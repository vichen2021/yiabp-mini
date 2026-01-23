export interface Template {
  id: string;
  name: string;
  templateStr: string;
  buildPath: string;
  remarks?: string | null;
  isDeleted: boolean;
  creationTime: string;
  creatorId?: string | null;
  lastModifierId?: string | null;
  lastModificationTime?: string | null;
}
