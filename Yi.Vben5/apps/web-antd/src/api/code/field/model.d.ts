export enum FieldTypeEnum {
  String = 0,
  Int = 1,
  Long = 2,
  Bool = 3,
  Decimal = 4,
  DateTime = 5,
  Guid = 6,
}

export interface Field {
  id: string;
  name: string;
  description?: string | null;
  orderNum: number;
  length: number;
  fieldType: FieldTypeEnum;
  tableId: string;
  isRequired: boolean;
  isKey: boolean;
  isAutoAdd: boolean;
  isPublic: boolean;
  isDeleted: boolean;
  creationTime: string;
  creatorId?: string | null;
  lastModifierId?: string | null;
  lastModificationTime?: string | null;
}
