// 密钥对生成 http://web.chacuo.net/netrsakeypair
import { useAppConfig } from '@vben/hooks';

import JSEncrypt from 'jsencrypt';

const appConfig = useAppConfig(
  import.meta.env,
  import.meta.env.PROD,
) as any;
const rsaPrivateKey =
  appConfig.rsaPrivateKey ?? import.meta.env.VITE_GLOB_RSA_PRIVATE_KEY;
const rsaPublicKey =
  appConfig.rsaPublicKey ?? import.meta.env.VITE_GLOB_RSA_PUBLIC_KEY;

/**
 * 加密
 * @param txt 需要加密的数据
 * @returns 加密后的数据
 */
export function encrypt(txt: string) {
  const instance = new JSEncrypt();
  instance.setPublicKey(rsaPublicKey);
  return instance.encrypt(txt);
}

/**
 * 解密
 * @param txt 需要解密的数据
 * @returns 解密后的数据
 */
export function decrypt(txt: string) {
  const instance = new JSEncrypt();
  instance.setPrivateKey(rsaPrivateKey);
  return instance.decrypt(txt);
}
