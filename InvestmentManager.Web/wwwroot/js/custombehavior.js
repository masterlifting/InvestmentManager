// CompanyFilter
var companySortNum = 1;
var sectorSortNum = 1;
var industrySortNum = 1;

function ChoiseSector(node) {
    const companyList = document.querySelector("#companyList").childNodes;
    for (let company of companyList) {
        if (company.nodeName !== "#text") {
            let sectorNode = company.firstElementChild.nextElementSibling.firstElementChild;

            if (node.checked === true) {
                if (sectorNode.value !== node.value) {
                    sectorNode.checked = false;
                    company.style.display = "none";
                }
                else {
                    sectorNode.checked = true;
                }
            }
            else {
                sectorNode.checked = false;
                company.style.display = "flex";
            }
        }
    }
}
function ChoiseIndustry(node) {
    const companyList = document.querySelector("#companyList").childNodes;
    for (let company of companyList) {
        if (company.nodeName !== "#text") {
            let industryNode = company.firstElementChild.nextElementSibling.nextElementSibling.firstElementChild.nextElementSibling;
            let sectorNode = company.firstElementChild.nextElementSibling.firstElementChild;

            if (node.checked === true) {
                if (industryNode.value !== node.value) {
                    industryNode.checked = false;
                    company.style.display = "none";
                }
                else {
                    industryNode.checked = true;
                    sectorNode.checked = false;
                    sectorNode.style.display = "none";
                }
            }
            else {

                industryNode.checked = false;
                company.style.display = "flex";

                sectorNode.checked = false;
                sectorNode.style.display = "flex";
            }
        }
    }
}
// CompanyReverse
function ReverseName() {
    const parrentCustom = document.querySelector("#companyList");
    let companyNames = new Map();
    let sortedNames = [];

    for (let company of parrentCustom.childNodes) {
        if (company.nodeName === "DIV") {
            let companyName = company.firstElementChild.firstElementChild.firstElementChild.nextElementSibling.innerText;
            companyNames.set(companyName, company);
            sortedNames.push(companyName);
        }
    }
    let child;
    while ((child = parrentCustom.firstChild) != null) {
        parrentCustom.removeChild(child);
    }

    sortedNames = companySortNum % 2 != 0 ? sortedNames.sort() : sortedNames.sort().reverse();

    for (let name of sortedNames) {
        parrentCustom.appendChild(companyNames.get(name));
    }
    companySortNum++;
}
// Sector Sort
function SortSector() {
    const parrentCustom = document.querySelector("#companyList");
    let companyNames = new Map();
    let sortedNames = [];
    let count = 0;
    for (let company of parrentCustom.childNodes) {
        if (company.nodeName === "DIV") {
            let sectorName = company.firstElementChild.nextElementSibling.firstElementChild.nextElementSibling.innerText;
            companyNames.set(sectorName + count, company);
            sortedNames.push(sectorName + count);
            count++;
        }
    }

    let child;
    while ((child = parrentCustom.firstChild) != null) {
        parrentCustom.removeChild(child);
    }

    sortedNames = sectorSortNum % 2 != 0 ? sortedNames.sort() : sortedNames.sort().reverse();

    for (let name of sortedNames) {
        parrentCustom.appendChild(companyNames.get(name));
    }
    sectorSortNum++;
}
// Industry Sort
function SortIndustry() {
    const parrentCustom = document.querySelector("#companyList");
    let companyNames = new Map();
    let sortedNames = [];
    let count = 0;
    for (let company of parrentCustom.childNodes) {
        if (company.nodeName === "DIV") {
            let sectorName = company.firstElementChild.nextElementSibling.nextElementSibling.firstElementChild.innerText;
            companyNames.set(sectorName + count, company);
            sortedNames.push(sectorName + count);
            count++;
        }
    }

    let child;
    while ((child = parrentCustom.firstChild) != null) {
        parrentCustom.removeChild(child);
    }

    sortedNames = industrySortNum % 2 != 0 ? sortedNames.sort() : sortedNames.sort().reverse();

    for (let name of sortedNames) {
        parrentCustom.appendChild(companyNames.get(name));
    }
    industrySortNum++;
}

// Hide/Show element
function isVisible(idElement) {
    let nodeStyle = document.querySelector(`#${idElement}`).style.display;

    if (nodeStyle == 'none') {
        document.querySelector(`#${idElement}`).style.display = 'flex';
        return false;
    }
    else {
        document.querySelector(`#${idElement}`).style.display = 'none';
        return true;
    }
}

// Show data report
function ShowReport(id) {
    let reportId = `_reports_${id}`;
    let showResult = isVisible(reportId);
    if (showResult === false) {
        $.get("/Financial/ReportHistoryPartial", { id: id }, function (data) {
            $("#" + reportId).replaceWith(data);
        });
    }
}


// GetBaseChartMethod
function GetChart(id, idDomElement) {
    idDomElement = `${idDomElement}${id}`;
    const chart = document.querySelector(`#${idDomElement}`).querySelector('svg');

    if (chart) {
        chart.remove();
        return;
    }

    $.get("/Financial/GetPrice", { id: id }, function (data) {
        const chartModel = {};
        chartModel.title = data.title;
        chartModel.xName = data.xName;
        chartModel.yName = data.yName;
        chartModel.points = [];
        for (let i of data.points) {
            chartModel.points.push({ x: new Date(i.Key), y: i.Value });
        };

        GetBaseChart(chartModel, idDomElement);

    }, "json");
}

function ShowFoundReport(id) {
    let display = document.querySelector(`#${id}`).style.display;
    document.querySelector(`#${id}`).style.display = display === 'none' ? 'flex' : 'none';
}
function ShowFoundCoefficient(id) {
    let display = document.querySelector(`#${id}`).style.display;
    document.querySelector(`#${id}`).style.display = display === 'none' ? 'inline' : 'none';
}

