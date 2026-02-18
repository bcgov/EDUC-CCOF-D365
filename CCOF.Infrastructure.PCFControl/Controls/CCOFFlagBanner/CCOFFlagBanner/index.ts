import type { IInputs, IOutputs } from "./generated/ManifestTypes";
import * as React from "react";
import * as ReactDOM from "react-dom";
import { MessageBanner } from "./MessageBanner";
import { MessageBarType } from "@fluentui/react";






/**
 * Represents a related notes flag record (minimal subset we need).
 */
interface NoteItem {
  id: string;
  name: string;
}

/**
 * Minimal shape for retrieveMultipleRecords results that we use.
 * PCF returns `{ entities: any[] }`, but we narrow to an indexable array.
 */
interface RetrieveMultipleResult<T extends object> {
  entities: T[];
}

/**
 * Narrowest shape we expect for a notes entity coming from the Web API.
 * We index properties dynamically using the strings configured via inputs,
 * so we keep the record index signature but avoid `any`.
 */


interface NotesEntityRecord extends Record<string, unknown> {
  /** Marker to avoid empty-interface lint while remaining structurally compatible */
  readonly __notesEntityRecordBrand__?: never;
}

interface ExpandedRecord extends Record<string, unknown> {
  /** Marker to avoid empty-interface lint while remaining structurally compatible */
  readonly __expandedRecordBrand__?: never;
}


/**
 * Runtime guard for strings from unknown objects.
 */
function asString(value: unknown): string | undefined {
  return typeof value === "string" ? value : undefined;
}

/**
 * Runtime guard for GUID strings (no braces).
 */
function normalizeGuid(value: unknown): string | undefined {
  const s = asString(value)?.replace(/[{}]/g, "");
  // Very loose GUID check – you can tighten if needed:
  return s && /^[0-9a-fA-F-]{36}$/.test(s) ? s : undefined;
}

export class CCOFFlagBanner implements ComponentFramework.StandardControl<IInputs, IOutputs> {
  private context!: ComponentFramework.Context<IInputs>;
  private container!: HTMLDivElement;
  private recordId?: string;
  private isRendered = false; // ✅ no explicit type, fixes no-inferrable-types

  // Inputs (with defaults), no explicit literal types to satisfy no-inferrable-types
  private notesEntity = "ccof_notesflag";
  private notesTitleAttr = "ccof_title";
  private intersectEntity="";
  private msgType = "";
  private relatedEntity="";
  private OrgId="";
private _orgLookup: {
    id: string;
    entityType: string;
} | null = null;

  
 private Scope: number[] = [];


  private maxTags = 50;
  private chipColor = "#eef2ff";
  private chipTextColor = "#223";
  private openOnClick = false;
    private readonly designMockEnabled = true;

  public init(
    context: ComponentFramework.Context<IInputs>,
    notifyOutputChanged: () => void,
    state: ComponentFramework.Dictionary,
    container: HTMLDivElement
  ): void {
    this.context = context;
    this.container = container;
    this.readInputs(context);

    // Get current record id from PCF context (prefer modern contextInfo)
    const idCandidate =
      (context.mode as { contextInfo?: { entityId?: string } }).contextInfo?.entityId ??
      // older typings:
      (context.mode as { recordId?: string }).recordId;

    this.recordId = normalizeGuid(idCandidate);
    void this.render();
  }

  public updateView(context: ComponentFramework.Context<IInputs>): void {
    
     const lookup = context.parameters.OrganizationAttribute.raw;

    // If lookup is missing, skip logic and wait for next updateView
    if (!lookup || !lookup[0] || !lookup[0].id) {
        console.log("Lookup not ready yet — waiting for next updateView");
        return;
    }
    const prevId = this.recordId;

    this.readInputs(context);

    const idCandidate =
      (context.mode as { contextInfo?: { entityId?: string } }).contextInfo?.entityId ??
      (context.mode as { recordId?: string }).recordId;

    this.recordId = normalizeGuid(idCandidate) ?? this.recordId;
   

    if (!this.isRendered || prevId !== this.recordId) {
      void this.render();
    }
  }

  public getOutputs(): IOutputs {
    return {};
  }

  public destroy(): void {
    ReactDOM.unmountComponentAtNode(this.container);
    // no-op
  }

  private readInputs(context: ComponentFramework.Context<IInputs>): void {
    // Read required/optional inputs with sensible defaults.
    this.notesEntity = context.parameters.primaryEntityLogicalName.raw?.trim() || "ccof_notesflag";
    this.notesTitleAttr = context.parameters.PrimaryNameAttribute.raw?.trim() || "ccof_title";
     this.msgType=context.parameters.messageType.raw;
     const maxTagsRaw = context.parameters.maxTags.raw;
    this.maxTags = typeof maxTagsRaw === "number" ? maxTagsRaw : 100;
  
const scopeRaw = context.parameters.Scope.raw ?? "[]";

  this.Scope = this.parseScopeArray(scopeRaw);
  
 const lookup = context.parameters.OrganizationAttribute.raw?.[0];

    if (lookup) {
        this._orgLookup = {
            id: lookup.id,
            entityType: lookup.entityType
        };

        this.OrgId = lookup.id;   // if you only need the ID
    } 

    console.log("org"+this._orgLookup); 

    const openClickRaw = context.parameters.openRecordOnClick.raw;
    this.openOnClick = typeof openClickRaw === "boolean" ? openClickRaw : false;
  }

  private async render(): Promise<void> {
    this.isRendered = true;
    this.container.innerHTML = "";

    const root = document.createElement("div");
    root.className = "notes-banner";
    root.style.setProperty("--chip-bg", this.chipColor);
    root.style.setProperty("--chip-fg", this.chipTextColor);
    this.container.appendChild(root);

    if (!this.recordId) {
      root.innerHTML = `<span class="notes-empty">Save the record to load notes.</span>`;
      return;
    }
       if (!this.notesEntity  ) {
      root.innerHTML = `<span class="notes-empty">Control not configured (intersect entity/lookup names missing).</span>`;
      return;
    }
   

    try {
      console.log("loadrelatedNotes");
      const items = await this.loadRelatedNotes(this.recordId);
       if (items.length === 0) {
        root.innerHTML = `<span class="notes-empty">No related notes.</span>`;
        return;
      }

      const limited = items.slice(0, this.maxTags);
      let messageType:MessageBarType;
      switch(this.msgType){
			case "Blocked":
				messageType = MessageBarType.blocked;
				break;
			case "Error":
				messageType = MessageBarType.error;
				break;
			default:
			case "Info":
				messageType = MessageBarType.info;
				break;
			case "SeverWarning":
				messageType = MessageBarType.severeWarning;
				break;
			case "Success":
				messageType = MessageBarType.success;
				break;
			case "Warning":
				messageType = MessageBarType.warning;
				break;
      }
      // Render all banners ONCE — do not render inside the loop
      const elements = limited.map((it) =>
  React.createElement(MessageBanner, {
    key: it.id,
    messageType: messageType,
    messageText: it.name,
    openOnClick: this.openOnClick,
    onOpenNote: this.openOnClick
      ? () =>
          this.context.navigation.openForm({
            entityName: this.notesEntity,
            entityId: it.id,
          })
      : undefined,
    noteId: it.id,
  } as React.ComponentProps<typeof MessageBanner>) // ✅ use typeof here
);

ReactDOM.render(React.createElement(React.Fragment, null, elements), this.container);
      
    
    
    } catch (e) {
      // We don’t expose the error to the user; we log for diagnostics.
      // Avoid `any` by using `unknown` and not indexing into it.
      // eslint-disable-next-line no-console
      console.error("NotesFlagBanner load error", e);
      root.innerHTML ='<span class="notes-empty">Failed to load related notes.</span>';
    }
  }

  /**
   * Loads `ccof_notesflag` titles via FetchXML through the N:N intersect entity.
   */
  private async loadRelatedNotes(fundingId: string): Promise<NoteItem[]> {
    // Derive the primary key of notes entity (<logicalname>id). If your schema differs,
    // consider adding another manifest property to supply the PK explicitly.
    const notesPk = `${this.notesEntity}id`;
    
  const today = this.formatDate(new Date());
  let fetchXml;
  let fetchxmlforapp;
  const orgId= normalizeGuid(this.OrgId);
  const scopeValuesXml = this.Scope
    .map(v => `<value>${v}</value>`)
    .join("");
   console.log("Raw entityType:", JSON.stringify(this._orgLookup?.entityType));

 /*const allowedTypes = ["account", "ccof_application"];

if (!allowedTypes.includes(this._orgLookup?.entityType.trim().toLowerCase() ?? "")) {
    orgId = fundingId;
}*/

  console.log("ORG ID from Scope"+orgId);
  console.log("Record ID from Scope"+fundingId);

 if (this.Scope.includes(3)){
     fetchXml = `
      <fetch version='1.0' mapping='logical' distinct='true'>
        <entity name='${this.notesEntity}'>
          <attribute name='${notesPk}' />
          <attribute name='${this.notesTitleAttr}' />
           <filter type='and'>
          <filter type='or'>
         <condition attribute='ccof_end_date' operator='ge' value='${today}'/>
         <condition attribute='ccof_end_date' operator='null'/>
         </filter>
          <filter type="or">
        <filter type="and">
          <condition attribute="ccof_organization" operator="eq" value="${orgId}" />
          <condition attribute="ccof_scope" operator="contain-values">
            <value>2</value>
          </condition>
        </filter>
        <filter type="and">
         <condition attribute="ccof_facilities" operator="like" value="%${fundingId}%" />
           <condition attribute="ccof_scope" operator="contain-values">
        <value>3</value>
      </condition>
       </filter>
       </filter>
    </filter>
               <order attribute='createdon' descending="true" />
        </entity>
      </fetch>`.trim();
 }
 else if (this.Scope.includes(0)){
 fetchxmlforapp=`<fetch top='1'>
  <entity name='ccof_application'>
    <attribute name='ccof_applicationid' />
    <attribute name='ccof_organization' />
      <filter>
      <condition attribute="ccof_applicationid" operator="eq" value="${orgId}" />
    </filter>
  </entity>
</fetch>
`;
console.log("application fetch"+fetchxmlforapp);

const result = await this.context.webAPI.retrieveMultipleRecords(
    "ccof_application",
    `?fetchXml=${encodeURIComponent(fetchxmlforapp)}`
);
let orgGuid = null;
console.log("test");

if (result.entities.length > 0) {
      const row = result.entities[0];

      // For FetchXML via WebAPI, you typically get the raw lookup under the logical name
      // But guard for either style to be safe:
      orgGuid =
        (row["ccof_organization"] as string) ??
        (row["_ccof_organization_value"] as string) ??
        null;
    }

 console.log("ORGanization"+orgGuid);

     fetchXml = `
      <fetch version='1.0' mapping='logical' distinct='true'>
        <entity name='${this.notesEntity}'>
          <attribute name='${notesPk}' />
          <attribute name='${this.notesTitleAttr}' />
          <filter type='or'>
         <condition attribute='ccof_end_date' operator='ge' value='${today}'/>
         <condition attribute='ccof_end_date' operator='null'/>
         </filter>
        <condition attribute="ccof_organization" operator="eq" value="${orgGuid}" />
           <condition attribute="ccof_scope" operator="contain-values">
        <value>4</value>
      </condition>
       </filter>
               <order attribute='createdon' descending="true" />
        </entity>
      </fetch>`.trim();
 }
 else{
   fetchXml = `
      <fetch version='1.0' mapping='logical' distinct='true'>
        <entity name='${this.notesEntity}'>
          <attribute name='${notesPk}' />
          <attribute name='${this.notesTitleAttr}' />
          <filter type='and'>
      <filter type='or'>
       <condition attribute='ccof_end_date' operator='ge' value='${today}'/>
         <condition attribute='ccof_end_date' operator='null'/>
      </filter>
      <condition attribute="ccof_organization" operator="eq" value="${orgId}" />
      <condition attribute="ccof_scope" operator="contain-values">
        ${scopeValuesXml}
      </condition>
       </filter>
          <order attribute='createdon' descending="true" />
        </entity>
      </fetch>`.trim();

 }
      console.log(fetchXml);

    const result = await this.context.webAPI.retrieveMultipleRecords(
      this.notesEntity,
      `?fetchXml=${encodeURIComponent(fetchXml)}`
    );

    // Narrow result type without `any`
    const typed: RetrieveMultipleResult<NotesEntityRecord> = result as unknown as RetrieveMultipleResult<NotesEntityRecord>;
    const { entities } = typed;

    const items: NoteItem[] = [];

    for (const rec of entities) {
       
      const idVal = rec[notesPk];
      const nameVal = rec[this.notesTitleAttr];

      const id = asString(idVal);
      const name = asString(nameVal);
      if (id && name) {
        items.push({ id, name });
      }
    }

    return items;
  }

  private formatDate(date: Date): string {
    const yyyy = date.getFullYear();
    const mm = String(date.getMonth() + 1).padStart(2, "0");
    const dd = String(date.getDate()).padStart(2, "0");
    return `${yyyy}-${mm}-${dd}`;
}


private parseScopeArray(raw: string | null | undefined): number[] {
  if (!raw || raw.trim() === "") return [];

  // Try JSON first
  try {
    const parsed = JSON.parse(raw);
    if (Array.isArray(parsed)) {
      return parsed
        .map(v => (typeof v === "number" ? v : Number(v)))
        .filter(n => Number.isFinite(n));
    }
  } catch {
    // ignore and try CSV
  }

  // CSV fallback: "1, 2, 3"
  return raw
    .split(",")
    .map(s => Number(s.trim()))
    .filter(n => Number.isFinite(n));
}


}