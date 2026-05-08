/**
 * useSignalR — LifeQuest gerçek zamanlı bildirim hook'u (MISS-007)
 *
 * Kullanım:
 *   useSignalR({ onNotification, onProactiveMessage })
 *
 * Token sessionStorage'dan alınır. Bağlantı kopması durumunda
 * withAutomaticReconnect() otomatik yeniden bağlanmayı sağlar.
 */
import { useEffect, useRef } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

const HUB_URL = (import.meta.env.VITE_API_BASE_URL || "http://localhost:5020/api")
    .replace("/api", "") + "/hubs/notification";

/**
 * @param {object} handlers
 * @param {function} handlers.onNotification     - Yeni bildirim geldiğinde çağrılır
 * @param {function} [handlers.onProactiveMessage] - Chatbot proaktif mesaj geldiğinde çağrılır
 */
export const useSignalR = ({ onNotification, onProactiveMessage } = {}) => {
    const connectionRef = useRef(null);

    useEffect(() => {
        const token = sessionStorage.getItem("token");
        if (!token) return;

        const connection = new HubConnectionBuilder()
            .withUrl(HUB_URL, { accessTokenFactory: () => token })
            .withAutomaticReconnect([0, 2000, 5000, 10000]) // retry intervals in ms
            .configureLogging(LogLevel.Warning)
            .build();

        if (onNotification) {
            connection.on("ReceiveNotification", onNotification);
        }

        if (onProactiveMessage) {
            connection.on("ReceiveProactiveMessage", onProactiveMessage);
        }

        connection.onreconnecting(() => {
            console.warn("[SignalR] Bağlantı koptu, yeniden bağlanılıyor...");
        });

        connection.onreconnected(() => {
            console.info("[SignalR] Yeniden bağlanıldı.");
        });

        connection.onclose((err) => {
            if (err) console.error("[SignalR] Bağlantı kapatıldı:", err);
        });

        let isMounted = true;

        connection.start()
            .then(() => {
                if (isMounted) console.info("[SignalR] Bağlandı →", HUB_URL);
            })
            .catch((err) => {
                // Sadece hala mount edilmişsek VE hata "negotiation" hatası değilse hata bas
                const isNegotiationError = err?.message?.includes("negotiation") || err?.name === "AbortError";
                if (isMounted && !isNegotiationError) {
                    console.error("[SignalR] Bağlantı başarısız:", err);
                }
            });

        connectionRef.current = connection;

        return () => {
            isMounted = false;
            connection.stop().catch(() => {});
        };
    }, []); // token değişirse sayfa zaten yeniden yüklenir (logout/login)

    return connectionRef;
};

export default useSignalR;
