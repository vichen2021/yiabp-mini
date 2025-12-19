#!/bin/bash
kill -9 $(lsof -t -i:19002)
echo "Yi-进程已关闭"
