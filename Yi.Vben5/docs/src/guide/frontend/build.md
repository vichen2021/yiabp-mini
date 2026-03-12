# 构建部署

## 构建

### 生产构建

```bash
pnpm build:antd
```

构建产物位于 `apps/web-antd/dist` 目录。

### 分析构建

```bash
pnpm run build:analyze
```

生成可视化分析报告，查看模块依赖和体积。

## 环境配置

### 生产环境

修改 `.env.production`：

```ini
# 后端 API 地址
VITE_GLOB_API_URL=https://api.example.com/api

# SignalR Hub 地址
VITE_GLOB_HUB_URL=https://api.example.com/hub

# 开启 SignalR
VITE_GLOB_SIGNALR_ENABLE=true
```

### 非根目录部署

修改 `.env.production`：

```ini
# 基础路径
VITE_BASE=/admin/
```

## Nginx 配置

```nginx
server {
    listen 80;
    server_name example.com;
    
    root /var/www/dist;
    index index.html;
    
    # SPA 路由支持
    location / {
        try_files $uri $uri/ /index.html;
    }
    
    # API 代理
    location /api/ {
        proxy_pass https://api.example.com/api/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
    
    # SignalR WebSocket 代理
    location /hub/ {
        proxy_pass https://api.example.com/hub/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
    }
    
    # 静态资源缓存
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
```

## Docker 部署

```dockerfile
FROM node:20-alpine AS builder
WORKDIR /app
COPY . .
RUN npm install -g pnpm
RUN pnpm install
RUN pnpm build:antd

FROM nginx:alpine
COPY --from=builder /app/apps/web-antd/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

## 相关文档

- [配置项](/guide/frontend/configuration) - 环境变量配置
- [快速开始](/guide/frontend/quick-start) - 开发入门
