function GetBaseChart(chartModel, idDomElement) {
    const xLenght = 1200;
    const yLenght = 500;
    const margin = 50
    // chart object
    const title = chartModel.title;
    const xName = chartModel.xName;
    const yName = chartModel.yName;
    const points = chartModel.points; // data

    // create svg container
    let svg = d3.select(`#${idDomElement}`)
        .append('svg')
        .attr('class', 'axis')
        .attr('width', xLenght)
        .attr('height', yLenght);

    const xAxisLenght = xLenght - (2 * margin);
    const yAxisLenght = yLenght - (2 * margin);

    const data = ConfiguringChartData(points, margin, xAxisLenght, yAxisLenght);

    // positioning axis
    svg.append('g')
        .attr("transform",  // сдвиг оси вниз и вправо
            "translate(" + margin + "," + (yLenght - margin) + ")")
        .call(data.xAxis);

    svg.append('g')
        .attr("transform", // сдвиг оси вниз и вправо на margin
            "translate(" + margin + "," + margin + ")")
        .call(data.yAxis);

    DrowingChartDataType(svg, data.scaleData);
};

function ConfiguringChartData(points, margin, xAxisLenght, yAxisLenght) {

    // convert data to view
    const xData = [];
    const yData = [];

    for (let i of points) {
        xData.push(i.x);
        yData.push(i.y);
    }

    // create interpolation function
    const scaleX = d3.scaleTime()
        .domain([d3.min(xData), d3.max(xData)])
        .range([0, xAxisLenght]);

    const scaleY = d3.scaleLinear()
        .domain([d3.min(yData), d3.max(yData)])
        .rangeRound([yAxisLenght, 0]);

    // масштабирование реальных данных в данные для нашей координатной системы
    const scaleData = [];
    for (i = 0; i < points.length; i++)
        scaleData.push({ x: scaleX(points[i].x) + margin, y: scaleY(points[i].y) + margin });

    // drow axis
    const xAxis = d3.axisBottom().ticks(12).tickFormat(d3.timeFormat('%B%y')).scale(scaleX);
    const yAxis = d3.axisLeft().ticks(20).scale(scaleY);

    return { xAxis: xAxis, yAxis: yAxis, scaleData: scaleData };
}

function DrowingChartDataType(svg, scaleData) {
    // рисуем линии
    svg.append("g")
        .append("path")
        .datum(scaleData)
        .attr("fill", "none")
        .style("stroke", "white")
        .style("stroke-width", 2)
        .attr('d', d3.line()
            .x(function (d) { return d.x; })
            .y(function (d) { return d.y; }));
};
