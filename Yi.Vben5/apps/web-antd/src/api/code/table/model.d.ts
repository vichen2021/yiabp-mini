import type { Field } from '../field/model';

export interface Table {
  id: string;
  name: string;
  description?: string | null;
  fields?: Field[] | null;
  isDeleted: boolean;
  creationTime: string;
  creatorId?: string | null;
  lastModifierId?: string | null;
  lastModificationTime?: string | null;
}
