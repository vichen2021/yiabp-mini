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
  /** Bucket 不存在时是否自动创建 */
  createContainerIfNotExists: boolean;
}
