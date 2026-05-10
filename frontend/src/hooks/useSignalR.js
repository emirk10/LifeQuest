import { useEffect } from "react";
import { useSignalRContext } from "../context/SignalRContext";

/**
 * useSignalR — LifeQuest gerçek zamanlı bildirim hook'u (v2 - Singleton Context tabanlı)
 */
export const useSignalR = ({ onNotification, onProactiveMessage } = {}) => {
    const { on, off } = useSignalRContext();

    useEffect(() => {
        const handleNotification = (notif) => {
            if (onNotification) onNotification(notif);
        };

        const handleProactive = (message) => {
            // Plan P2-T1: openChatBot custom event dispatch
            window.dispatchEvent(new CustomEvent("openChatBot", { detail: message }));
            if (onProactiveMessage) onProactiveMessage(message);
        };

        on("ReceiveNotification", handleNotification);
        on("ReceiveProactiveMessage", handleProactive);

        return () => {
            off("ReceiveNotification", handleNotification);
            off("ReceiveProactiveMessage", handleProactive);
        };
    }, [onNotification, onProactiveMessage, on, off]);
};

export default useSignalR;
