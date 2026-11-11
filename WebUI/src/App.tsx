import React, { useState } from "react";
import { LoginPage } from "./pages/LoginPage";
import { ProjectListPage } from "./pages/ProjectListPage";
import { Sparkles, LogOut } from "lucide-react";

export const App: React.FC = () => {
  const [token, setToken] = useState(() => localStorage.getItem("token"));
  const [user, setUser] = useState(() => localStorage.getItem("user") || "syhong");

  const handleLoginSuccess = (u: string, t: string) => {
    setUser(u);
    setToken(t);
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("X-Auth-Token");
    localStorage.removeItem("user");
    setToken(null);
  };

  if (!token) {
    return <LoginPage onLoginSuccess={handleLoginSuccess} />;
  }

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col">
      {/* Header Bar */}
      <header className="border-b border-slate-800 bg-slate-900/90 backdrop-blur-md sticky top-0 z-50">
        <div className="max-w-7xl mx-auto px-6 h-14 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="p-1.5 bg-indigo-600 rounded-lg text-white">
              <Sparkles className="h-4 w-4" />
            </div>
            <span className="font-extrabold text-sm tracking-tight text-white">설문온 NewProject</span>
            <span className="text-[10px] px-2 py-0.5 rounded bg-indigo-950/60 text-indigo-400 border border-indigo-800/50 font-mono font-bold">
              v1.0 Standard
            </span>
          </div>

          <div className="flex items-center gap-4 text-xs">
            <span className="text-slate-400">
              접속자: <strong className="text-white font-mono">{user}</strong>님
            </span>
            <button
              onClick={handleLogout}
              className="px-3 py-1.5 rounded-lg border border-slate-700 bg-slate-800 hover:bg-slate-700 text-slate-300 text-xs font-semibold flex items-center gap-1.5 transition-all cursor-pointer"
            >
              <LogOut className="h-3.5 w-3.5" />
              <span>로그아웃</span>
            </button>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="flex-1 max-w-7xl w-full mx-auto p-6">
        <ProjectListPage />
      </main>
    </div>
  );
};

export default App;
