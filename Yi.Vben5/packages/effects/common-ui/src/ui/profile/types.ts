import type { BasicUserInfo } from '@vben/types';

export interface Props {
  tabs: {
    label: string;
    value: string;
  }[];
  title?: string;
  userInfo: BasicUserInfo | null;
}

export interface FormSchemaItem {
  description: string;
  fieldName: string;
  label: string;
  value: boolean;
}

export interface SettingProps {
  formSchema: FormSchemaItem[];
}
