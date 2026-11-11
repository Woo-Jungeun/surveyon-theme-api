import React, { useState } from "react";
import { Lock, User, LogIn, Sparkles } from "lucide-react";
import { api } from "../api/client";

interface LoginPageProps {
  onLoginSuccess: (user: string, token: string) => void;
}

export const LoginPage: React.FC<LoginPageProps> = ({ onLoginSuccess }) => {
  const [username, setUsername] = useState("syhong");
  const [password, setPassword] = useState("2402033");
  const [isLoading, setIsLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setErrorMsg(null);

    try {
      const res = await api.post("/auth/login", { username, password });
      const data = res.data;

      if (data.success === "777" && data.data?.token) {
        const token = data.data.token;
        const user = data.data.userId || username;
        localStorage.setItem("X-Auth-Token", token);
        localStorage.setItem("token", token);
        localStorage.setItem("user", user);
        onLoginSuccess(user, token);
      } else {
        setErrorMsg(data.message || `[${data.success}] 로그인에 실패했습니다.`);
      }
    } catch (err: any) {
      setErrorMsg("서버 연결에 실패했습니다.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-950 p-4">
      <div className="w-full max-w-md bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-6">
        <div className="text-center space-y-2">
          <div className="inline-flex items-center justify-center p-3 bg-indigo-950/60 border border-indigo-800/50 rounded-xl text-indigo-400 mb-1 shadow-inner">
            <Sparkles className="h-6 w-6 text-indigo-400" />
          </div>
          <h1 className="text-xl font-bold text-white tracking-tight">설문온 NewProject 시스템</h1>
          <p className="text-xs text-slate-400">인증 토큰 기반 대시보드 로그인</p>
        </div>

        {errorMsg && (
          <div className="p-3 bg-rose-950/50 border border-rose-800/60 rounded-xl text-xs text-rose-300 font-mono">
            ⚠️ {errorMsg}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1">
            <label className="text-xs font-semibold text-slate-300">아이디</label>
            <div className="relative">
              <User className="absolute left-3 top-3 h-4 w-4 text-slate-500" />
              <input
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
                className="w-full pl-9 pr-4 py-2.5 bg-slate-950 border border-slate-800 rounded-xl text-xs text-white placeholder-slate-500 focus:outline-none focus:border-indigo-500 transition-colors"
                placeholder="아이디 입력"
              />
            </div>
          </div>

          <div className="space-y-1">
            <label className="text-xs font-semibold text-slate-300">비밀번호</label>
            <div className="relative">
              <Lock className="absolute left-3 top-3 h-4 w-4 text-slate-500" />
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                className="w-full pl-9 pr-4 py-2.5 bg-slate-950 border border-slate-800 rounded-xl text-xs text-white placeholder-slate-500 focus:outline-none focus:border-indigo-500 transition-colors"
                placeholder="비밀번호 입력"
              />
            </div>
          </div>

          <button
            type="submit"
            disabled={isLoading}
            className="w-full py-3 bg-indigo-600 hover:bg-indigo-500 text-white rounded-xl text-xs font-bold transition-all shadow-md flex items-center justify-center gap-2 disabled:opacity-50 cursor-pointer"
          >
            {isLoading ? (
              <span>로그인 처리 중...</span>
            ) : (
              <>
                <LogIn className="h-4 w-4" />
                <span>로그인하기 (syhong)</span>
              </>
            )}
          </button>
        </form>
      </div>
    </div>
  );
};
