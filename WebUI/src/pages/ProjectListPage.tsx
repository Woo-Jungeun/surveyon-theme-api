import React, { useState, useEffect, useRef } from "react";
import { RefreshCw, FolderKanban, Search, CheckCircle, Clock, AlertCircle } from "lucide-react";
import { api } from "../api/client";
import { ProgressBar } from "../components/ProgressBar";

interface ProjectItem {
  id?: string;
  status?: string;
  projectpof?: string;
  projectPof?: string;
  projectnum?: string;
  projectNum?: string;
  projectname?: string;
  projectName?: string;
  register_userid?: string;
  registerUserId?: string;
  servername?: string;
  serverName?: string;
  register_date?: string;
  registerDate?: string;
  project_use_name?: string;
  project_update_date?: string;
  project_status?: string;
  projectStatus?: string;
  postgrecompletecount?: string;
}

export const ProjectListPage: React.FC = () => {
  const [user, setUser] = useState(() => localStorage.getItem("user") || "syhong");
  const [isSyncing, setIsSyncing] = useState(false);
  const [jobId, setJobId] = useState<string | null>(null);
  const [percent, setPercent] = useState(0);
  const [currentCount, setCurrentCount] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [statusMsg, setStatusMsg] = useState("");
  const [projects, setProjects] = useState<ProjectItem[]>([]);
  const [searchQuery, setSearchQuery] = useState("");

  const pollingRef = useRef<NodeJS.Timeout | null>(null);

  // 1. Start Sync API (gb: list_start)
  const handleStartSync = async () => {
    setIsSyncing(true);
    setPercent(0);
    setStatusMsg("동기화 작업 요청 중...");

    try {
      const res = await api.post("/project/sync", {
        gb: "list_start",
        user
      });

      const resData = res.data;
      if (resData.success === "777" && resData.resultjson?.jobId) {
        const jId = resData.resultjson.jobId;
        setJobId(jId);
        setStatusMsg(resData.resultjson.message || "동기화 작업을 시작했습니다.");
        startPolling(jId);
      } else {
        setStatusMsg(resData.message || "동기화 요청 실패");
        setIsSyncing(false);
      }
    } catch (err) {
      setStatusMsg("서버 동기화 연결 실패");
      setIsSyncing(false);
    }
  };

  // 2. Poll Progress API (gb: list_progress)
  const startPolling = (jId: string) => {
    if (pollingRef.current) clearInterval(pollingRef.current);

    pollingRef.current = setInterval(async () => {
      try {
        const res = await api.post("/project/sync", {
          gb: "list_progress",
          jobid: jId,
          user
        });

        const resData = res.data;
        if (resData.success === "777" && resData.resultjson) {
          const rJson = resData.resultjson;
          setPercent(rJson.percent || 0);
          if (rJson.currentCount) setCurrentCount(rJson.currentCount);
          if (rJson.totalCount) setTotalCount(rJson.totalCount);
          if (rJson.message) setStatusMsg(rJson.message);

          if (rJson.status === "completed" || rJson.percent === 100) {
            if (pollingRef.current) clearInterval(pollingRef.current);
            setIsSyncing(false);
            if (Array.isArray(rJson.resultjson)) {
              setProjects(rJson.resultjson);
            }
          }
        }
      } catch (err) {
        console.error("Polling error:", err);
      }
    }, 1500);
  };

  useEffect(() => {
    return () => {
      if (pollingRef.current) clearInterval(pollingRef.current);
    };
  }, []);

  const filteredProjects = projects.filter((p) => {
    const q = searchQuery.toLowerCase();
    const pName = p.projectname || p.projectName || "";
    const pNum = p.projectnum || p.projectNum || "";
    const pPof = p.projectpof || p.projectPof || "";
    const pUser = p.register_userid || p.registerUserId || p.project_use_name || "";
    const pServer = p.servername || p.serverName || "";
    return (
      !q ||
      pName.toLowerCase().includes(q) ||
      pNum.toLowerCase().includes(q) ||
      pPof.toLowerCase().includes(q) ||
      pUser.toLowerCase().includes(q) ||
      pServer.toLowerCase().includes(q)
    );
  });

  return (
    <div className="space-y-6">
      {/* Header Bar */}
      <div className="flex flex-wrap items-center justify-between gap-4 bg-slate-900 border border-slate-800 p-5 rounded-2xl shadow-sm">
        <div className="flex items-center gap-3">
          <div className="p-3 bg-indigo-950/80 border border-indigo-800/50 rounded-xl text-indigo-400">
            <FolderKanban className="h-6 w-6 text-indigo-400" />
          </div>
          <div>
            <h2 className="text-lg font-extrabold text-white">프로젝트 목록 동기화</h2>
            <p className="text-xs text-slate-400">중앙 서버(APIs/o/project) 연동 폴링 대시보드</p>
          </div>
        </div>

        <button
          type="button"
          onClick={handleStartSync}
          disabled={isSyncing}
          className="px-4 py-2.5 bg-indigo-600 hover:bg-indigo-500 text-white rounded-xl text-xs font-bold transition-all shadow-md flex items-center gap-2 disabled:opacity-50 cursor-pointer"
        >
          <RefreshCw className={`h-4 w-4 ${isSyncing ? "animate-spin" : ""}`} />
          <span>{isSyncing ? "동기화 수집 중..." : "프로젝트 동기화 시작"}</span>
        </button>
      </div>

      {/* Progress Bar Component */}
      {isSyncing && (
        <ProgressBar
          percent={percent}
          message={statusMsg}
          currentCount={currentCount}
          totalCount={totalCount}
        />
      )}

      {/* Filter & Table Bar */}
      <div className="bg-slate-900 border border-slate-800 rounded-2xl p-5 space-y-4 shadow-xs">
        <div className="flex items-center justify-between gap-4">
          <div className="relative w-full max-w-xs">
            <Search className="absolute left-3 top-2.5 h-4 w-4 text-slate-500" />
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="프로젝트명/번호 검색..."
              className="w-full pl-9 pr-4 py-2 bg-slate-950 border border-slate-800 rounded-xl text-xs text-white placeholder-slate-500 focus:outline-none focus:border-indigo-500"
            />
          </div>
          <span className="text-xs text-slate-400 font-mono">총 {filteredProjects.length}건 조회</span>
        </div>

        {/* Data Table */}
        <div className="overflow-x-auto rounded-xl border border-slate-800">
          <table className="w-full text-xs text-left border-collapse">
            <thead>
              <tr className="bg-slate-950 border-b border-slate-800 text-[11px] font-bold text-slate-400 uppercase">
                <th className="py-3 px-4">상태</th>
                <th className="py-3 px-4">웹프로젝트 번호</th>
                <th className="py-3 px-4">POF 번호</th>
                <th className="py-3 px-4">프로젝트명</th>
                <th className="py-3 px-4">등록자</th>
                <th className="py-3 px-4">서버정보</th>
                <th className="py-3 px-4">등록일자</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-800/60 font-mono">
              {filteredProjects.length === 0 ? (
                <tr>
                  <td colSpan={7} className="py-8 text-center text-xs text-slate-500">
                    {isSyncing ? "데이터를 동기화하는 중입니다..." : "상단 [프로젝트 동기화 시작] 버튼을 클릭하여 조회를 진행해 주세요."}
                  </td>
                </tr>
              ) : (
                filteredProjects.map((p, idx) => {
                  const pofNum = p.projectpof || p.projectPof || "-";
                  const projNum = p.projectnum || p.projectNum || "-";
                  const projName = p.projectname || p.projectName || "-";
                  const regUser = p.register_userid || p.registerUserId || p.project_use_name || "-";
                  const serverInfo = p.servername || p.serverName || "-";
                  const regDate = p.register_date || p.registerDate || p.project_update_date || "-";
                  const statusText = p.status || p.project_status || p.projectStatus || "완료";

                  return (
                    <tr key={p.id || idx} className="hover:bg-slate-800/40 transition-colors text-[11px]">
                      <td className="py-3 px-4 font-sans">
                        <span className="px-2 py-0.5 rounded text-[10px] font-bold bg-emerald-950/60 text-emerald-400 border border-emerald-800/50">
                          {statusText}
                        </span>
                      </td>
                      <td className="py-3 px-4 text-indigo-300 font-semibold">{projNum}</td>
                      <td className="py-3 px-4 text-slate-300">{pofNum}</td>
                      <td className="py-3 px-4 font-sans text-white font-bold">{projName}</td>
                      <td className="py-3 px-4 text-slate-400">{regUser}</td>
                      <td className="py-3 px-4 text-slate-400">{serverInfo}</td>
                      <td className="py-3 px-4 text-slate-400">{regDate}</td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};
