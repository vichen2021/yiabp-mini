import { requestClient } from '#/api/request';

/**
 * 发送短信验证码
 * @param phone 手机号
 * @returns void
 */
export function sendSmsCode(phone: string) {
  return requestClient.get<void>('/resource/sms/code', {
    params: { phone },
  });
}

/**
 * 发送邮件验证码
 * @param email 邮箱
 * @returns void
 */
export function sendEmailCode(email: string) {
  return requestClient.get<void>('/resource/email/code', {
    params: { email },
  });
}

/**
 * @param img 图片验证码 需要和base64拼接
 * @param isEnableCaptcha 是否开启
 * @param uuid 验证码ID
 */
export interface CaptchaResponse {
  isEnableCaptcha: boolean;
  img: string;
  uuid: string;
}

/**
 * 图片验证码
 * @returns resp
 */
export function captchaImage() {
  return requestClient.get<CaptchaResponse>('/account/captcha-image');
}
