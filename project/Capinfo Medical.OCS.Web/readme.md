1.部署程序注意修改应用程序池->[高级设置]->[进程模型]->[标识]--改为localsystem，否则无法实现导出excel功能；
2.下载文件设置IIS 的MIME配置后缀例如sql脚本 .sql ； MIME类型设置application/x-zip-compressed；