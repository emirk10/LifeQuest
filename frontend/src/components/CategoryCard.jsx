import React from "react";
import { useNavigate } from "react-router-dom";

const CategoryCard = ({ name, iconPath, taskCount }) => {
  const navigate = useNavigate();

  const showIcon = iconPath && iconPath.length < 5;

  return (
    <div
      onClick={() => navigate(`/tasks?category=${name}`)}
      className="group cursor-pointer relative"
    >
      <div className="bg-slate-900 border border-slate-800 p-6 rounded-2xl hover:border-cyan-500/50 transition-all duration-300 relative overflow-hidden">
        <div className="absolute top-0 right-0 p-2 opacity-0 group-hover:opacity-100 transition-opacity">
          <div className="w-2 h-2 bg-cyan-500 rounded-full shadow-[0_0_10px_cyan]"></div>
        </div>

        <div className="flex items-center gap-4">
          <div className="w-16 h-16 bg-slate-950 border border-slate-700 rounded-lg flex items-center justify-center relative group-hover:border-cyan-500 transition-colors shadow-lg shrink-0">
            <div className="absolute top-0 left-0 w-2 h-2 border-t-2 border-l-2 border-slate-600 group-hover:border-cyan-400"></div>
            <div className="absolute bottom-0 right-0 w-2 h-2 border-b-2 border-r-2 border-slate-600 group-hover:border-cyan-400"></div>

            {showIcon ? (
              <span className="text-3xl">{iconPath}</span>
            ) : (
              <span className="text-3xl font-black text-slate-700 group-hover:text-cyan-400 transition-colors font-mono">
                {name ? name.charAt(0).toUpperCase() : "?"}
              </span>
            )}
          </div>

          <div className="flex-1">
            <h3 className="text-white font-bold text-lg group-hover:text-cyan-400 transition-colors">
              {name}
            </h3>
            <p className="text-slate-500 text-xs font-mono mt-1">
              PKT: {taskCount} GÖREV
            </p>
          </div>
        </div>

        <div className="absolute bottom-0 left-0 h-1 bg-gradient-to-r from-cyan-500 to-blue-600 w-0 group-hover:w-full transition-all duration-500"></div>
      </div>
    </div>
  );
};

export default CategoryCard;
