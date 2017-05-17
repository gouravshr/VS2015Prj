exec sys.sp_cdc_help_jobs

exec sys.sp_cdc_change_job @job_type = 'cleanup', @retention = 1052640

exec sys.sp_cdc_help_jobs
