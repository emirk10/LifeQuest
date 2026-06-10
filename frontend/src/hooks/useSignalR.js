import { useEffect } from "react";
import { useSignalRContext } from "../context/SignalRContext";




export const useSignalR = ({ onNotification, onProactiveMessage } = {}) => {
    const { on, off } = useSignalRContext();

    useEffect(() => {
        const handleNotification = (notif) => {
            if (onNotification) onNotification(notif);
        };

        const handleProactive = (message) => {
            
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
