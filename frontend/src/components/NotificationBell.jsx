import React, { useState, useEffect } from "react";
import axiosClient from "../api/axiosClient";
import useSignalR from "../hooks/useSignalR";

const NotificationBell = () => {
    const [notifications, setNotifications] = useState([]);
    const [isOpen, setIsOpen] = useState(false);
    const [unreadCount, setUnreadCount] = useState(0);

    // Fetch initial notifications
    useEffect(() => {
        const fetchNotifications = async () => {
            try {
                const response = await axiosClient.get("/notifications");
                setNotifications(response.data.grouped || {});
                setUnreadCount(response.data.unreadCount || 0);
            } catch (err) {
                console.error("Failed to fetch notifications", err);
            }
        };

        fetchNotifications();
    }, []);

    // Setup SignalR — real-time notifications via useSignalR hook (MISS-007)
    useSignalR({
        onNotification: () => {
            // Re-fetch to keep grouping correct
            axiosClient.get("/notifications").then(response => {
                setNotifications(response.data.grouped || {});
                setUnreadCount(response.data.unreadCount || 0);
            });
        },
    });

    const toggleOpen = () => {
        if (!isOpen && unreadCount > 0) {
            axiosClient.patch("/notifications/mark-all-as-read").then(() => {
                setUnreadCount(0);
                // Lokal olarak hepsini okundu işaretle (Gruplanmış veride derin güncelleme)
                setNotifications(prev => {
                    const newGroups = { ...prev };
                    Object.keys(newGroups).forEach(key => {
                        newGroups[key] = newGroups[key].map(n => ({ ...n, isRead: true }));
                    });
                    return newGroups;
                });
            });
        }
        setIsOpen(!isOpen);
    };

    return (
        <div className="relative">
            <button 
                onClick={toggleOpen}
                className="relative p-2 text-slate-300 hover:text-white hover:bg-slate-800 rounded-full transition-colors"
                title="Bildirimler"
            >
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
                </svg>
                {unreadCount > 0 && (
                    <span className="absolute top-1 right-1 w-2.5 h-2.5 bg-red-500 rounded-full animate-bounce" />
                )}
            </button>

            {isOpen && (
                <div className="absolute right-0 mt-2 w-80 bg-slate-800 border border-slate-700 rounded-xl shadow-2xl z-50 overflow-hidden text-sm animate-fade-in">
                    <div className="p-3 bg-slate-700/50 border-b border-slate-600 font-semibold text-slate-100 flex justify-between">
                        Bildirimler
                        {unreadCount > 0 && <span className="text-xs bg-indigo-500/20 text-indigo-300 px-2 py-0.5 rounded-full">{unreadCount} yeni</span>}
                    </div>
                    <div className="max-h-96 overflow-y-auto">
                        {Object.keys(notifications).length === 0 ? (
                            <div className="p-4 text-center text-slate-400">Bildirim bulunmuyor.</div>
                        ) : (
                            Object.entries(notifications).map(([type, group]) => (
                                <div key={type}>
                                    <div className="bg-slate-900/50 px-4 py-2 text-[10px] font-black text-indigo-400 uppercase tracking-widest border-b border-slate-700/30">
                                        {type === 'Social' ? '🤝 SOSYAL' : type === 'System' ? '⚙️ SİSTEM' : type === 'Task' ? '⚔️ GÖREV' : `🔔 ${type.toUpperCase()}`}
                                    </div>
                                    {group.map((n, idx) => (
                                        <div key={n.id || idx} className={`p-4 border-b border-slate-700/50 hover:bg-slate-700/40 transition-colors ${!n.isRead ? 'bg-slate-700/20' : ''}`}>
                                            <h4 className="font-medium text-slate-200 mb-1">{n.title}</h4>
                                            <p className="text-slate-400 text-xs">{n.message}</p>
                                            <span className="text-[10px] text-slate-500 mt-2 block">
                                                {new Date(n.createdAt).toLocaleString('tr-TR')}
                                            </span>
                                        </div>
                                    ))}
                                </div>
                            ))
                        )}
                    </div>
                </div>
            )}
        </div>
    );
};

export default NotificationBell;
