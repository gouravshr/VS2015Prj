/* ----------------------------------------------------------
    Shared functions used by the self service app.
   ----------------------------------------------------------*/

function autopost() {
    $('.autopost').each(function (idx) {
        $(this).change(function () {
            $(this).closest('form').submit();
        });
    });
}

function stripgrid() {
    $(".strip tbody tr:odd").addClass("odd");
}

$.postJSON = function (url, data, callback) {
    $.ajax({
      url: url,
      type: 'POST',
      dataType: 'json',
      data: data,
      success: callback
    });
};

function userAction( url , data ) {
    data = data || null 
    //$.postJSON(url, null,
    $.postJSON(url, data,
        function (data) {
            if (data.warning) { alert(data.warning); }
            // go with the dumb direct locaiton.href setting way that works cross browser.. the jQuery attr() had issues before
            if (data.redirect) { window.location.href = data.redirect; }
        });
}

/* ------- supervisor approval ----------------- */
function showTicketApproveConfirmation() {
    $("#ticket_decision").hide();
    $("#ticket_approve").slideDown();
}

function cancelTicketApprove() {
    $("#ticket_approve").hide();
    $("#ticket_decision").fadeIn('slow');
}

function ticketApprove() {
    var data = $("#supervisor_decision").serialize();
    userAction("/Ticket/TicketApprove/", data);
}

function showTicketAbortConfirmation() {
    $("#ticket_decision").hide();
    $("#ticket_abort").slideDown();
}

function cancelTicketAbort() {
    $("#ticket_abort").hide();
    $("#ticket_decision").fadeIn('slow');
}

function ticketAbort() {
    var data = $("#supervisor_decision").serialize();
    userAction("/Ticket/TicketAbort/", data);
}

/* ------- DBA validation overwrite ------- */
function showDbaManualValidationConfirmation() {
    $('#dba_overwrite_decision').hide();
    $('#dba_overwrite_confirm').slideDown();
}

function showDbaValidationNoThanks() {
    $('#dba_overwrite_decision').hide();
    $('#dba_overwrite_nope').slideDown();
}

function kickOffDbaManualValidation(id ) {
    userAction("/Ticket/KickOffDbaManualValidation/" + id);
}

function cancelOverwriteConfirmAndBackToDecision() {
    $('#dba_overwrite_confirm').hide();
    $('#dba_overwrite_decision').fadeIn();
}

function noDbaValidationAndTerminateFlow( id ) {
    userAction("/Ticket/NoDbaValidationAndTerminateFlow/" + id);
}

function cancelTerminateRequestAndBackToDecision() {
    $('#dba_overwrite_nope').hide();
    $('#dba_overwrite_decision').fadeIn();
}

function dba_approve(id) {
    var data = $("#dba_decision").serialize()
    userAction("/Ticket/DbaApproveSqlScript/" + id, data );
}

function dba_reject(id) {
    var data = $("#dba_decision").serialize()
    userAction("/Ticket/DbaRejectSqlScript/" + id, data );
}

/* ------- push to stage ----------------- */
function showPushToStageConfirmation() {
    $("#push_to_stage_menu").hide();
    $("#stage_push").slideDown(); 
} 

function cancelPushToStage() {
    $("#stage_push").hide(); 
    $("#push_to_stage_menu").fadeIn('slow');
}

//Accept userid and password for oracle for executing the script into the Staging
function pushToStage() {
    var data = $("#user_account").serialize();
    userAction("/Ticket/PushToStaging/", data);
}

function pushToStage(id) {
    userAction("/Ticket/PushToStaging/" + id);
}

function showStageAbortConfirmation() {
    $("#push_to_stage_menu").hide();
    $("#stage_abort").slideDown();
}

function cancelStageAbort() {
    $("#stage_abort").hide(); 
    $("#push_to_stage_menu").fadeIn('slow');
}

function stageAbort(id) {
    userAction("/Ticket/UserAbort/" + id);
}

/* -------- stage exec validation ---- */
function showStageConfirmInfo() { 
    $("#stage_confirm_menu").hide();
    $("#stage_confirm").slideDown();
}

function cancelStageConfirm() { 
    $("#stage_confirm").hide();
    $("#stage_confirm_menu").fadeIn('slow');
}

function stageConfirm(id) {
    userAction("/Ticket/ConfirmStagingExecution/" + id);
}

function showStageRejectAndRollbackInfo() { 
    $("#stage_confirm_menu").hide();
    $("#stage_reject_rollback").slideDown();
}

function cancelStageReject() {
    $("#stage_reject_rollback").hide();
    $("#stage_confirm_menu").fadeIn('slow');
}

function stageRejectAndRollback(id) {
    userAction("/Ticket/RejectStagingExecution/" + id);
}

function stageRejectAndRollback() {
    var data = $("#user_account").serialize();
    userAction("/Ticket/RejectStagingExecution/", data);
}

/* ---------------- push tof production ------------ */
function showPushToProdInfo() {
    $("#push_prod_menu").hide();
    $("#prod_push").slideDown();
}

function cancelPushToProd() {
    $("#prod_push").hide();
    $("#push_prod_menu").fadeIn('slow');
}

function pushToProd(id) {
    userAction("/Ticket/PushToProd/" + id);
}

//Accept userid and password for oracle for executing the script into the Production
function pushToProd() {
    var data = $("#user_account").serialize();
    userAction("/Ticket/PushToProd/", data);
}

function showProdUserAbortInfo() {
    $("#push_prod_menu").hide();
    $("#prod_abort").slideDown();
}

function cancelProdAbort() {
    $("#prod_abort").hide();
    $("#push_prod_menu").fadeIn('slow');
}

function prodAbort(id) {
    // for now, just use the same server side abort...
    userAction("/Ticket/UserAbort/" + id);
}

/* -------------- production exec validation --- */

function showProdConfirmInfo() { 
    $("#prod_confirm_menu").hide();
    $("#prod_confirm").slideDown();
}

function showProdRejectAndRollbackInfo() { 
    $("#prod_confirm_menu").hide();
    $("#prod_reject_rollback").slideDown();
}

function cancelProdConfirm(){ 
    $("#prod_confirm").hide();
    $("#prod_confirm_menu").fadeIn('slow');
}

function cancelProdReject() {
    $("#prod_reject_rollback").hide();
    $("#prod_confirm_menu").fadeIn('slow');
}

function prodConfirm(id) {
    userAction("/Ticket/ConfirmProdExecution/" + id);
}

function prodConfirm() {
    var data = $("#user_account").serialize();
    userAction("/Ticket/ConfirmProdExecution/", data);
}

function prodRejectAndRollback(id) {
    userAction("/Ticket/RejectProdExecution/" + id);
}

function prodRejectAndRollback() {
    var data = $("#user_account").serialize();
    userAction("/Ticket/RejectProdExecution/", data);
}

function getScriptStatus(id) {
    debugger;
    userAction("/Ticket/ScriptExecutionStatus/" + id);
}