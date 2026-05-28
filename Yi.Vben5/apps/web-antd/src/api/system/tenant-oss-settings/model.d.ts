export interface TenantOssSetting {
  /** OSS Provider，可选值：FileSystem / Aliyun */
  provider?: string;
  /** 文件存储路径前缀 */
  pathPrefix?: string;
  /** 阿里云 AccessKeyId */
  accessKeyId?: string;
  /** 阿里云 AccessKeySecret。读取时始终为空，写入时传明文 */
  accessKeySecret?: string;
  /** 阿里云 OSS Endpoint */
  endpoint?: string;
  /** 阿里云 OSS Bucket 名称 */
  containerName?: string;
  /** 阿里云 OSS 自定义访问域名，为空时使用默认 Bucket 域名 */
  customDomain?: string;
  /** Bucket 不存在时是否自动创建 */
  createContainerIfNotExists: boolean;
}
