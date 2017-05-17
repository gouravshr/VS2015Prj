exec sys.sp_cdc_help_jobs

exec sys.sp_cdc_change_job @job_type = 'cleanup', @retention = 5256000

exec sys.sp_cdc_help_jobs
