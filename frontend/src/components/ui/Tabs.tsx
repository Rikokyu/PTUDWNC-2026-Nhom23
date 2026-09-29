import React from "react";
import { cn } from "@/lib/utils";

export interface TabItem {
  id: string;
  label: string;
  count?: number;
}

export interface TabsProps {
  tabs: TabItem[];
  activeTab: string;
  onChange: (id: string) => void;
  className?: string;
  variant?: "underline" | "pills";
}

export const Tabs: React.FC<TabsProps> = ({
  tabs,
  activeTab,
  onChange,
  className,
  variant = "underline",
}) => {
  if (variant === "pills") {
    return (
      <div className={cn("inline-flex items-center gap-1 bg-slate-100 p-1 rounded-lg border border-slate-200/80", className)}>
        {tabs.map((tab) => {
          const isActive = tab.id === activeTab;
          return (
            <button
              key={tab.id}
              onClick={() => onChange(tab.id)}
              className={cn(
                "px-3.5 py-1.5 text-xs font-semibold rounded-md transition-all duration-150 flex items-center gap-2 select-none",
                isActive
                  ? "bg-white text-content-primary shadow-xs"
                  : "text-content-secondary hover:text-content-primary"
              )}
            >
              <span>{tab.label}</span>
              {typeof tab.count === "number" && (
                <span
                  className={cn(
                    "text-[10px] px-1.5 py-0.2 rounded-full font-bold",
                    isActive ? "bg-brand-light text-brand-hover" : "bg-slate-200 text-slate-600"
                  )}
                >
                  {tab.count}
                </span>
              )}
            </button>
          );
        })}
      </div>
    );
  }

  return (
    <div className={cn("border-b border-slate-200 flex items-center gap-8 overflow-x-auto no-scrollbar", className)}>
      {tabs.map((tab) => {
        const isActive = tab.id === activeTab;
        return (
          <button
            key={tab.id}
            onClick={() => onChange(tab.id)}
            className={cn(
              "py-3 font-semibold text-sm transition-colors relative flex items-center gap-2 whitespace-nowrap select-none",
              isActive
                ? "text-brand"
                : "text-content-secondary hover:text-content-primary"
            )}
          >
            <span>{tab.label}</span>
            {typeof tab.count === "number" && (
              <span
                className={cn(
                  "text-xs px-2 py-0.5 rounded-full font-medium",
                  isActive
                    ? "bg-brand-light text-brand-hover"
                    : "bg-slate-100 text-content-tertiary"
                )}
              >
                {tab.count}
              </span>
            )}
            {isActive && (
              <span className="absolute bottom-0 left-0 right-0 h-0.5 bg-brand rounded-t-full" />
            )}
          </button>
        );
      })}
    </div>
  );
};
