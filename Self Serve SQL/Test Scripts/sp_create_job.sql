sp_add_job
    @job_name = N'Ad hoc Sales Data', 
    @enabled = 1,
    @description = N'Ad hoc sales data',
    @owner_login_name = N'françoisa',
    @notify_level_eventlog = 2,
    @notify_level_email = 2,
    @notify_level_netsend = 2,
    @notify_level_page = 2,
    @notify_email_operator_name = N'François Ajenstat',
    @notify_netsend_operator_name = N'François Ajenstat', 
    @notify_page_operator_name = N'François Ajenstat',
    @delete_level = 1 ;