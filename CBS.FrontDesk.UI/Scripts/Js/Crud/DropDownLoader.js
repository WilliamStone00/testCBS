function LoadOperationEventAttributes(id, OperationEventAttributeId) {
    console.log(id);

    console.log(OperationEventAttributeId);
    var url = "/TransactionConfiguration/Ajaxloader?Key=" + id;
    FillDropDownAjaxCall(url, OperationEventAttributeId, "---Select operation event attribute---")
}