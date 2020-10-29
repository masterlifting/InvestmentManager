function uploadBrokerReports(inputFileId) {
    document.querySelector(`#${inputFileId}`).click();
}
function setDefaultFilter(filterId) {
    document.querySelector(`#${filterId}`).options[0].selected = true;
}