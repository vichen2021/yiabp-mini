/**
 * 文件存储配置 - 对应后端 OSS 配置实体属性名（接口仍调用 /resource/oss/config）
 */
export interface FileConfig {
  ossConfigId: number;
  configKey: string;
  accessKey: string;
  secretKey: string;
  bucketName: string;
  prefix: string;
  endpoint: string;
  domain: string;
  isHttps: string;
  region: string;
  status: string;
  ext1: string;
  remark: string;
  accessPolicy: string;
}
