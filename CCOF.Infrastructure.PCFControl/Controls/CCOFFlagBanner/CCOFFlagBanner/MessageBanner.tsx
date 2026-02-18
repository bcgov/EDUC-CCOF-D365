// MessageBanner.tsx
import * as React from "react";
import { MessageBar, MessageBarType, Link } from "@fluentui/react";

export interface MessageBannerProps  {
  messageType: MessageBarType;           // accept the enum (flexible)
  messageText: string;                   // a single title per bar
  openOnClick: boolean;                  // whether it should be clickable
  onOpenNote?: (id?: string) => void;    // optional click handler (may receive an id)
  noteId?: string;                       // pass the note id if you want openOnClick
};

export const MessageBanner: React.FC<MessageBannerProps> = ({
  messageType,
  messageText,
  openOnClick,
  onOpenNote,
  noteId
}) => {
  if (!messageText) return null;

  return (
    <MessageBar
      messageBarType={messageType}
      isMultiline={true}
      dismissButtonAriaLabel="Dismiss"
    >
      {openOnClick && onOpenNote ? (
        <Link onClick={() => onOpenNote(noteId)} title={messageText}>
          {messageText}
        </Link>
      ) : (
        <span title={messageText}>{messageText}</span>
      )}
    </MessageBar>
  );
};
export default MessageBanner;