import React from "react";

interface ProgressBarProps {
  percent: number;
  message?: string;
  currentCount?: number;
  totalCount?: number;
}

export const ProgressBar: React.FC<ProgressBarProps> = ({
  percent,
  message,
  currentCount,
  totalCount
}) => {
  const safePercent = Math.min(100, Math.max(0, percent));

  return (
    <div className="w-full bg-slate-900 border border-slate-800 rounded-xl p-4 space-y-2 shadow-sm">
      <div className="flex items-center justify-between text-xs font-semibold">
        <span className="text-indigo-400 flex items-center gap-1.5">
          <span className="relative flex h-2 w-2">
            <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-indigo-400 opacity-75"></span>
            <span className="relative inline-flex rounded-full h-2 w-2 bg-indigo-500"></span>
          </span>
          <span>{message || "동기화 진행 중..."}</span>
        </span>
        <span className="font-mono text-indigo-300 font-bold">{safePercent}%</span>
      </div>

      {/* Progress Bar Container */}
      <div className="w-full bg-slate-800 rounded-full h-2.5 overflow-hidden">
        <div
          className="bg-gradient-to-r from-indigo-500 to-purple-500 h-2.5 rounded-full transition-all duration-300 ease-out shadow-xs"
          style={{ width: `${safePercent}%` }}
        />
      </div>

      {totalCount !== undefined && totalCount > 0 && (
        <div className="text-[10px] text-right font-mono text-slate-400">
          [{currentCount || 0} / {totalCount}] 처리 완료
        </div>
      )}
    </div>
  );
};
