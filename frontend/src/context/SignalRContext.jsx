import React, { createContext, useContext, useEffect, useRef, useState } from 'react';
import { HubConnectionBuilder, LogLevel, HttpTransportType } from '@microsoft/signalr';
import { useAuth } from './authContext';

const SignalRContext = createContext();

const HUB_URL = (import.meta.env.VITE_API_BASE_URL || "http://localhost:5020/api")
    .replace("/api", "") + "/hubs/notification";

export const SignalRProvider = ({ children }) => {
    const { user } = useAuth();
    const connectionRef = useRef(null);
    const [isConnected, setIsConnected] = useState(false);
    const handlersRef = useRef([]); // Array of { eventName, callback }

    useEffect(() => {
        if (!user) {
            if (connectionRef.current) {
                connectionRef.current.stop();
                connectionRef.current = null;
                setIsConnected(false);
            }
            return;
        }

        if (connectionRef.current) return; // Already connecting/connected

        const connection = new HubConnectionBuilder()
            .withUrl(HUB_URL, {
                withCredentials: true,
                transport: HttpTransportType.WebSockets | HttpTransportType.LongPolling,
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000])
            .configureLogging(LogLevel.Warning)
            .build();

        connection.onreconnecting(() => setIsConnected(false));
        connection.onreconnected(() => setIsConnected(true));
        connection.onclose(() => setIsConnected(false));

        // Register any queued handlers to this new connection
        handlersRef.current.forEach(({ eventName, callback }) => {
            connection.on(eventName, callback);
        });

        connection.start()
            .then(() => {
                console.info("[SignalR] Global connection established.");
                setIsConnected(true);
            })
            .catch(err => {
                console.error("[SignalR] Connection failed:", err);
                setIsConnected(false);
            });

        connectionRef.current = connection;

        return () => {
            if (connectionRef.current) {
                handlersRef.current.forEach(({ eventName, callback }) => {
                    connectionRef.current.off(eventName, callback);
                });
                connectionRef.current.stop();
                connectionRef.current = null;
                setIsConnected(false);
            }
        };
    }, [user]);

    const on = (eventName, callback) => {
        // Prevent duplicate handler registrations in queue
        const exists = handlersRef.current.some(h => h.eventName === eventName && h.callback === callback);
        if (!exists) {
            handlersRef.current.push({ eventName, callback });
        }
        if (connectionRef.current) {
            connectionRef.current.on(eventName, callback);
        }
    };

    const off = (eventName, callback) => {
        handlersRef.current = handlersRef.current.filter(
            h => !(h.eventName === eventName && h.callback === callback)
        );
        if (connectionRef.current) {
            connectionRef.current.off(eventName, callback);
        }
    };

    return (
        <SignalRContext.Provider value={{ connection: connectionRef.current, isConnected, on, off }}>
            {children}
        </SignalRContext.Provider>
    );
};

export const useSignalRContext = () => useContext(SignalRContext);
