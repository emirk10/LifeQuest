import React, { useState, useEffect, useRef } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import axiosClient from "../api/axiosClient";

const NotificationBell = () => {
    const [notifications, setNotifications] = useState([]);
    const [isOpen, setIsOpen] = useState(false);
    const [unreadCount, setUnreadCount] = useState(0);
    const connectionRef = useRef(null);

    // Fetch initial notifications
    useEffect(() => {
        const fetchNotifications = async () => {
            try {
                const response = await axiosClient.get("/notifications");
                const notifs = response.data.notifications || [];
                setNotifications(notifs);
                setUnreadCount(notifs.filter(n => !n.isRead).length);
            } catch (err) {
                console.error("Failed to fetch notifications", err);
            }
        };

        fetchNotifications();
    }, []);

    // Setup SignalR
    useEffect(() => {
        const token = localStorage.getItem("token");
        if (!token) return;

        const baseUrl = (import.meta.env.VITE_API_BASE_URL || "https://localhost:7071/api").replace("/api", "");
        const newConnection = new HubConnectionBuilder()
            .withUrl(`${baseUrl}/hubs/notification`, { 
                accessTokenFactory: () => token 
            })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        newConnection.start()
            .then(() => {
                console.log("SignalR Connected");
                newConnection.on("ReceiveNotification", (notification) => {
                    setNotifications(prev => [notification, ...prev]);
                    setUnreadCount(prev => prev + 1);
                    
                    // İsteğe bağlı browser notification atılabilir
                    /* if ("Notification" in window && Notification.permission === "granted") {
                        new Notification(notification.title, { body: notification.message });
                    } */
                });
            })
            .catch(e => console.log("Connection failed: ", e));

        connectionRef.current = newConnection;

        return () => {
            if (connectionRef.current) {
                connectionRef.current.stop();
            }
        };
    }, []);

    const toggleOpen = () => {
        if (!isOpen && unreadCount > 0) {
            axiosClient.patch("/notifications/mark-all-as-read").then(() => {
                setUnreadCount(0);
                setNotifications(prev => prev.map(n => ({...n, isRead: true})));
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
                        {notifications.length === 0 ? (
                            <div className="p-4 text-center text-slate-400">Bildirim bulunmuyor.</div>
                        ) : (
                            notifications.map((n, idx) => (
                                <div key={n.id || idx} className={`p-4 border-b border-slate-700/50 hover:bg-slate-700/40 transition-colors ${!n.isRead ? 'bg-slate-700/20' : ''}`}>
                                    <h4 className="font-medium text-slate-200 mb-1">{n.title}</h4>
                                    <p className="text-slate-400 text-xs">{n.message}</p>
                                    <span className="text-[10px] text-slate-500 mt-2 block">
                                        {new Date(n.createdAt).toLocaleString('tr-TR')}
                                    </span>
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
