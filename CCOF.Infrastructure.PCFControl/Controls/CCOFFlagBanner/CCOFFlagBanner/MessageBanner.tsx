// MessageBanner.tsx
import * as React from "react";
import { MessageBar, MessageBarType, Link } from "@fluentui/react";

export interface MessageBannerProps {
    messageType: MessageBarType;           // accept the enum (flexible)
    messageText: string;                  // a single title per bar
    // Legacy props (optional, ignored)
    openOnClick?: boolean;
    onOpenNote?: (id?: string) => void;
    noteId?: string;
    // pass the note id if you want openOnClick
};

export const MessageBanner: React.FC<MessageBannerProps> = ({
    messageType,
    messageText
}) => {
    if (!messageText) return null;

    return (
        <MessageBar
            messageBarType={messageType}
            isMultiline={true}
            dismissButtonAriaLabel="Dismiss">
            <span title={messageText}>{messageText}</span>
        </MessageBar>
    );
};
export default MessageBanner;