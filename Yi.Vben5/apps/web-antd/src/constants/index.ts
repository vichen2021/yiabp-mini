export const DEFAULT_TENANT_ID = '000000';

export const DictEnum = {
  SYS_COMMON_STATUS: 'sys_common_status',
  SYS_DB_TYPE: 'sys_db_type',
  SYS_DEVICE_TYPE: 'sys_device_type',
  SYS_GRANT_TYPE: 'sys_grant_type',
  SYS_NORMAL_DISABLE: 'sys_normal_disable',
  SYS_NOTICE_STATUS: 'sys_notice_status',
  SYS_NOTICE_TYPE: 'sys_notice_type',
  SYS_OPER_TYPE: 'sys_oper_type',
  SYS_OSS_ACCESS_POLICY: 'oss_access_policy',
  SYS_SHOW_HIDE: 'sys_show_hide',
  SYS_USER_SEX: 'sys_user_sex',
  SYS_YES_NO: 'sys_yes_no',
  WF_BUSINESS_STATUS: 'wf_business_status',
  WF_FORM_TYPE: 'wf_form_type',
  WF_TASK_STATUS: 'wf_task_status',
} as const;

export type DictEnumKey = keyof typeof DictEnum;
