export interface Tenant {
  id: string;
  name: string;
  entityVersion: number;
  tenantConnectionString: string;
  dbType: number;
  state: boolean;
  isDeleted: boolean;
  creationTime: string;
  creatorId: string | null;
  lastModifierId: string | null;
  lastModificationTime: string | null;
  contactUserName?: string;
  contactPhone?: string;
  expireTime?: string;
  accountCount?: number;
  domain?: string;
  address?: string;
  licenseNumber?: string;
  intro?: string;
  remark?: string;
  // 表单专用（新建租户时的初始管理员）
  username?: string;
  password?: string;
  tenantId?: string;
}
