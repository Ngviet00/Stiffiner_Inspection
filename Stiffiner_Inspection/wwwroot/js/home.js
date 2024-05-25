"use strict";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/homeHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

$(function () {
    var timeouts = [null, null, null, null];
    var deepcores = [null, null, null, null];
    var clientConnects = [null, null, null, null];
    var previousTray = [];
    var resetPLC = 1;

    const noDataTimeLogRow = $('.time-log-no-data');

    var statusPLC = null;
    var statusClient = null;

    var ConnectPC = [null, null, null, null]
    var CamPC = [null, null, null, null]
    var DeepLearningPC = [null, null, null, null]

    for (let i = 1; i <= 4; i++) {
        clearTimeout(timeouts[i]);
        clearTimeout(deepcores[i]);
        clearTimeout(clientConnects[i]);
    }


    const STATUS_PLC = Object.freeze({
        'EMG': 0,
        'START': 1,
        'STOP': 2,
        'ALARM': 3,
        'DISCONNECTED': 4
    });

    const STATUS_RESULT = Object.freeze({
        'OK': 1,
        'NG': 2,
        'EMPTY': 3,
    });

    const SYSTEM_STATUS_CLIENT = Object.freeze({
        'RUNNING': 1,
        'PAUSE': 2,
        'ERROR': 3,
    });

    const CLIENT = Object.freeze({
        'CLIENT_1': 1,
        'CLIENT_2': 2,
        'CLIENT_3': 3,
        'CLIENT_4': 4,
    });

    //====================================================== EVENT REALTIME ======================================================

    //connection start
    connection.start()
        .then(() => {
            console.log('Connection established!');
            //UpdateStatisticalCalculations();

            for (let i = 1; i <= 4; i++) {
                connection.invoke("ChangeStatusCamVisionBusy", i, 0)
                    .then(function (res) {
                        console.log(`Turn off cam PC ${i} successfully`)
                    })
                    .catch(function (err) {
                        console.error("Error calling API:", err.toString());
                    });

                connection.invoke("ChangeDeepCoreVisionBusy", i, 0)
                    .then(function (res) {
                        console.log(`Turn off deep core PC ${i} successfully`)
                    })
                    .catch(function (err) {
                        console.error("Error calling API:", err.toString());
                    });

                connection.invoke("ChangeConnectVisionBusy", i, 0)
                    .then(function (res) {
                        console.log(`Turn off connect PC ${i} successfully`)
                    })
                    .catch(function (err) {
                        console.error("Error calling API:", err.toString());
                    });
            }
        })
        .catch((err) => {
            console.error(err.toString())
        });

    //event receive data
    connection.on("ReceiveData", (data) => {
        const noDataResultLogRow = $('.result-log-no-data');
        if (noDataResultLogRow) {
            noDataResultLogRow.remove();
        }

        previousTray.push(data);

        appendResultLog(data);
    });

    //event check status camera pc
    connection.on("ChangeCAM", (client_id, status) => {
        clearTimeout(timeouts[client_id]);

        $(".dot-cam-" + client_id).removeClass('cam-is-active').css("background", status == 1 ? '#0ad90a' : '#b6b9b6');

        if (CamPC[client_id] != status) {
            appendTimeLog(getCurrentDateTime(), "Client", `Cam PC ${client_id} is connected`)
            CamPC[client_id] = status;
        }

        timeouts[client_id] = setTimeout(function () {
            console.log('cam turn off', client_id, status);

            $(".dot-cam-" + client_id).css("background", '#b6b9b6');
            connection.invoke("ChangeStatusCamVisionBusy", client_id, 0)
                .then(function (res) {
                    CamPC[client_id] = null;
                    appendTimeLog(getCurrentDateTime(), "Client", `CAM PC ${client_id} is disconnected!`)
                })
                .catch(function (err) {
                    console.error("Error calling API:", err.toString());
                    location.reload();
                });
        }, 4000);
    });

    //event deep learning
    connection.on("deepcore", (client_id, status) => {
        clearTimeout(deepcores[client_id]);

        $(".dot-deep-core-" + client_id).css("background", status == 1 ? '#0ad90a' : '#b6b9b6');

        if (DeepLearningPC[client_id] != status) {
            appendTimeLog(getCurrentDateTime(), "Client", `Deep Learning PC ${client_id} is connected`)
            DeepLearningPC[client_id] = status;
        }

        deepcores[client_id] = setTimeout(function () {
            $(".dot-deep-core-" + client_id).css("background", '#b6b9b6');
            connection.invoke("ChangeDeepCoreVisionBusy", client_id, 0)
                .then(function (res) {
                    appendTimeLog(getCurrentDateTime(), "Client", `Deep learning PC ${client_id} is disconnected!`)
                    DeepLearningPC[client_id] = null;
                })
                .catch(function (err) {
                    console.error("Error calling API:", err.toString());
                    location.reload();
                });
        }, 4000);
    });

    //event check client connect
    connection.on("ChangeClientConnect", (clientId) => {
        clearTimeout(clientConnects[clientId])
        $(".dot-connect-" + clientId).css("background", "#0ad90a")

        if (ConnectPC[clientId] == null) {
            appendTimeLog(getCurrentDateTime(), "Client", `Client PC ${clientId} is connected`)
            ConnectPC[clientId] = 1;
        }
        clientConnects[clientId] = setTimeout(function () {
            $(".dot-connect-" + clientId).css("background", '#b6b9b6')
            connection.invoke("ChangeConnectVisionBusy", clientId, 0)
                .then(function (res) {
                    ConnectPC[clientId] = null
                    appendTimeLog(getCurrentDateTime(), "Client", `Client PC ${clientId} is disconnected!`)
                })
                .catch(function (err) {
                    console.error("Error calling API:", err.toString());
                    location.reload();
                });
        }, 4000);
    });

    //event change plc
    connection.on("ChangeStatusPLC", (status) => {
        let _status = $('#value-plc-status');
        let _message = $('#error-plc-status')

        status == STATUS_PLC.ALARM ? _message.removeClass('d-none') : _message.addClass('d-none');

        if (status == STATUS_PLC.DISCONNECTED) {
            _status.css("color", "#222222").css("background", "#E6E6E6").text("Disconnect");
            $('#select-model').prop('disabled', false);
            $('.btn-reload-model').prop('disabled', false);
            $('.btn-clear-data').prop('disabled', false);
            $('.mode-run').prop('disabled', false);
            $('#form-setting .button-delete-all-data').prop('disabled', false)

            if (statusPLC != status) {
                appendTimeLog(getCurrentDateTime(), "PLC", `PLC Disconnected!`);
                statusPLC = status;
            }

            return;
        }

        if (status == STATUS_PLC.ALARM) {
            _status.css("color", "#3C3C3C").css("background", "#FFCA08").text("Alarm");
            if (statusPLC != status) {
                appendTimeLog(getCurrentDateTime(), "PLC", `PLC Alarm!`);
                statusPLC = status;
            }
            return;
        }

        if (status == STATUS_PLC.EMG) {
            _status.css("color", "#E34440").css("background", "#FD53083D").text("EMG");
            if (statusPLC != status) {
                appendTimeLog(getCurrentDateTime(), "PLC", `PLC EMG!`);
                statusPLC = status;
            }
            return;
        }

        if (status == STATUS_PLC.START) {
            _status.css("color", "#ffffff").css("background", "#49A31D").text("Start");
            $('#select-model').prop('disabled', true);
            $('.btn-reload-model').prop('disabled', true);
            $('.btn-clear-data').prop('disabled', true);
            $('.mode-run').prop('disabled', true);
            $('#form-setting .button-delete-all-data').prop('disabled', true)

            if (statusPLC != status) {
                appendTimeLog(getCurrentDateTime(), "PLC", `PLC Start!`);
                statusPLC = status;
            }

            return;
        }

        if (status == STATUS_PLC.STOP) {
            _status.css("color", "#ffffff").css("background", "#E4491D").text("Stop");
            $('#select-model').prop('disabled', false);
            $('.btn-reload-model').prop('disabled', false);
            $('.btn-clear-data').prop('disabled', false);
            $('.mode-run').prop('disabled', false);
            $('#form-setting .button-delete-all-data').prop('disabled', false)

            if (statusPLC != status) {
                appendTimeLog(getCurrentDateTime(), "PLC", `PLC Stop!`);
                statusPLC = status;
            }

            return;
        }
    });

    //event change system client
    connection.on("ChangeStatusSystemClient", (status, message) => {
        let _status = $('#value-system-status');
        let _message = $('#error-system-status');

        status == SYSTEM_STATUS_CLIENT.ERROR ? _message.removeClass('d-none') : _message.addClass('d-none');

        if (status == SYSTEM_STATUS_CLIENT.RUNNING) {
            _status.css("color", "#ffffff").css("background", "#49A31D").text("Running");
            _message.addClass('d-none');
            if (statusClient != status) {
                appendTimeLog(getCurrentDateTime(), "Client", "Client is running!");
                statusClient = status;
            }
            return;
        }
        else {
            _status.css("color", "#344054").css("background", "#E6E6E6").text("Pause");
            _message.addClass('d-none');
            if (statusClient != status) {
                appendTimeLog(getCurrentDateTime(), "Client", "Client is pause!");
                statusClient = status;
            }
            return;
        }
    });

    //event plc reset
    connection.on("PLCReset", async (value) => {
        //set resetPLC = 1 avoid duplicate
        if (value == 1 && resetPLC == 1) {
            resetPLC++;
            resetCurrentTray();
            appendPreviousTray();

            //clear result log
            $('#result-log table tbody').html(`
                <tr class="result-log-no-data">
                    <td colspan="12" class="w-100 text-lg-center text-dark fw-bold mt-1" style="font-size: 14px;">No data</td>
                </tr>
            `)
        } else {
            resetPLC = 1;
        }
    });

    connection.on("ListModels", (results) => {
        let options = '<option value="" selected disabled>Choose Model</option>';
        let optionsFormSearch = '<option value="" selected>Choose Model</option>';

        results.forEach(item => {
            options += `<option value="${item}">${item}</option>`;
            optionsFormSearch += `<option value="${item}">${item}</option>`;
        });

        $('#select-model').html(options);
        $('#form-search-model').html(optionsFormSearch);

        alert('Please choose model!');
    });

    connection.on("RefreshData", function () {
        UpdateStatisticalCalculations();
    });

    //====================================================== CONFIG CHART ======================================================
    var ctx = document.getElementById('pie-chart').getContext('2d');

    var valueChart = document.getElementById('data-chart-percent');

    var values = [
        parseFloat(valueChart.getAttribute('data-percent-chart-ok')),
        parseFloat(valueChart.getAttribute('data-percent-chart-ng')),
        parseFloat(valueChart.getAttribute('data-percent-chart-empty')),
    ];

    if (values[0] == 0 && values[1] == 0 && values[2] == 0) {
        values = [100, 0, 0]
    }

    var myPieChart = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: ["OK", "NG", "Empty"],
            datasets: [{
                data: values,
                backgroundColor: [
                    '#66b032', '#e4491d', '#9F9F9F',
                ]
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            title: {
                display: false,
                text: null,
            },
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        color: '#667085',
                    }
                },
                tooltip: {
                    enabled: false,
                },
                datalabels: {
                    formatter: (value, context) => {
                        const datapoints = context.chart.data.datasets[0].data;
                        function totalSum(total, datapoint) {
                            return total + datapoint;
                        }
                        const totalValue = datapoints.reduce(totalSum, 0);
                        const percentageValue = (value / totalValue * 100).toFixed(2);
                        return `${percentageValue}%`;
                    },
                    color: '#ffffff',
                    font: {
                        weight: 'bold',
                        size: 15,
                    },
                    align: 'end',
                    anchor: 'center'
                }
            }
        },
        plugins: [ChartDataLabels]
    });

    //====================================================== FUNCTION ======================================================
    function UpdateStatisticalCalculations() {
        connection.invoke("UpdateStatistical", "UpdateStatictical")
            .then(function (res) {
                $('#total-tray-ea').html(formatNumberWithDot(res.totalTray));
                $('#total-ea').html(`${formatNumberWithDot(res.total)}<span class="">&nbspEA</span>`);
                $('#total-ok-ea').html(`${formatNumberWithDot(res.totalOK)}<span class="">&nbspEA</span>`);
                $('#total-ng-ea').html(`${formatNumberWithDot(res.totalNG)}<span class="">&nbspEA</span>`);
                $('#total-empty-ea').html(`${formatNumberWithDot(res.totalEmpty)}<span class="">&nbspEA</span>`);

                $('#percent-ok').html(`${res.percentChartOk} %`);
                $('#percent-ng').html(`${res.percentChartNG} %`);
                $('#percent-empty').html(`${res.percentChartEmpty} %`);

                if (res.percentChartOk == 0 && res.percentChartNG == 0 && res.percentChartEmpty == 0) {
                    res.percentChartOk = 100;
                }

                myPieChart.data.datasets[0].data = [res.percentChartOk, res.percentChartNG, res.percentChartEmpty];
                myPieChart.data.labels = ["OK", "NG", "Empty"];
                myPieChart.update('none');
            })
            .catch(function (err) {
                console.error("Error calling API:", err.toString());
            })
            .finally(function () {
                //setTimeout(UpdateStatisticalCalculations, 2500)
            });
    }

    function appendPreviousTray() {
        let client1 = "";
        let client2 = "";
        let client3 = "";
        let client4 = "";

        if (previousTray.length > 0) {

            previousTray.forEach(item => {

                let clientId = item.client_id;

                let rs = item.result == STATUS_RESULT.OK ? 'OK'
                    : (item.result == STATUS_RESULT.NG) ? 'NG'
                        : (item.result == STATUS_RESULT.EMPTY) ? 'Empty' : '';

                if (clientId == CLIENT.CLIENT_1) {
                    client1 +=
                        `<span class="${rs.toLowerCase()}">
                            ${rs}
                        </span>`;
                    return;
                }

                if (clientId == CLIENT.CLIENT_2) {
                    client2 +=
                        `<span class="${rs.toLowerCase()}">
                            ${rs}
                        </span>`;
                    return;
                }

                if (clientId == CLIENT.CLIENT_3) {
                    client3 +=
                        `<span class="${rs.toLowerCase()}">
                            ${rs}
                        </span>`;
                    return;
                }

                if (clientId == CLIENT.CLIENT_4) {
                    client4 +=
                        `<span class="${rs.toLowerCase()}">
                            ${rs}
                        </span>`;
                    return;
                }
            });
        } else {
            for (let i = 1; i <= 80; i++) {
                if (i <= 20) {
                    client1 += `<span class="wait">Wait</span>`;
                    return;
                }

                if (i > 20 && i <= 40) {
                    client2 += `<span class="wait">Wait</span>`;
                    return;
                }

                if (i > 40 && i <= 60) {
                    client3 += `<span class="wait">Wait</span>`;
                    return;
                }

                if (i > 60 && i <= 80) {
                    client4 += `<span class="wait">Wait</span>`;
                    return;
                }
            }
        }

        $('#result .previous-tray .checking-tray-left .ng-ok .left-tray').html(client1)
        $('#result .previous-tray .checking-tray-left .ng-ok .right-tray').html(client2)
        $('#result .previous-tray .checking-tray-right .ng-ok .left-tray').html(client3)
        $('#result .previous-tray .checking-tray-right .ng-ok .right-tray').html(client4)

        previousTray = [];
    }

    function appendTimeLog(time, type, message) {
        if (noDataTimeLogRow) {
            noDataTimeLogRow.remove();
        }

        $("#time-log table tbody").prepend(`
            <tr>
                <td class="max-w90">${convertDate(time)}</td>
                <td class="max-w105">${type}</td>
                <td>${message}</td>
            </tr>
        `);
    }

    function resetCurrentTray() {
        let client1 = '';
        let client2 = '';
        let client3 = '';
        let client4 = '';

        for (let i = 1; i <= 20; i++) {
            client1 += `<span class="ok left-area-${i}">Wait</span>`;
            client2 += `<span class="ng left-line-${i}">Wait</span>`;
            client3 += `<span class="ok right-area-${i}">Wait</span>`;
            client4 += `<span class="ng right-line-${i}">Wait</span>`;
        }

        $('#result .current-tray .checking-tray-left .ng-ok .left-tray').empty().append(client1)
        $('#result .current-tray .checking-tray-left .ng-ok .right-tray').empty().append(client2)
        $('#result .current-tray .checking-tray-right .ng-ok .left-tray').empty().append(client3)
        $('#result .current-tray .checking-tray-right .ng-ok .right-tray').empty().append(client4)
    }

    function appendResultLog(data) {
        $("#result-log table tbody").prepend(`
            <tr>
                <td>${convertDate(data.time)}</td >
                <td class="text-capitalize">${data.model}</td>
                <td>${data.tray}</td>
                <td class="text-capitalize">${data.side}</td>
                <td>${data.index}</td>
                <td class="text-capitalize">${data.camera}</td>
                <td class="status-item ${data.result === 1 ? "text-success" : "text-danger"}">
                    ${data.result == STATUS_RESULT.OK ? "OK" : (data.result == STATUS_RESULT.NG ? "NG" : (data.result == STATUS_RESULT.EMPTY ? "EMPTY" : ""))}
                </td>
                <td class="detail-error">
                    ${data.result == STATUS_RESULT.OK || data.result == STATUS_RESULT.EMPTY ? "-" : data.error ?? "-"}
                </td>
            </tr>
        `);

        if (data.result === STATUS_RESULT.OK) {
            $(`.${data.side}-${data.camera}-${data.index}`).css("background", "#66b032").text("OK");
            return;
        }

        if (data.result === STATUS_RESULT.NG) {
            $(`.${data.side}-${data.camera}-${data.index}`).css("background", "#e4491d").text("NG");
            return;
        }

        if (data.result === STATUS_RESULT.EMPTY) {
            $(`.${data.side}-${data.camera}-${data.index}`).css("background", "#9F9F9F").text("Empty");
            return;
        }
    }

    function convertDate(date) {
        let hours = date.substr(11, 2);
        let minutes = date.substr(14, 2);
        let seconds = date.substr(17, 2);
        let milliseconds = date.substr(20, 3);

        return hours + ":" + minutes + ":" + seconds + ":" + milliseconds;
    }

    function GetResult(item) {
        if (item.resultArea == 1 && item.resultLine == 1) {
            return 'OK';
        }

        if (item.resultArea == 3 && item.resultLine == 3) {
            return 'Empty';
        }

        return 'NG';
    }

    function getCurrentDateTime() {
        return new Date(new Date().getTime() + (7 * 60 * 60 * 1000)).toISOString().slice(0, 23) + 'Z';
    }

    function formatNumberWithDot(number) {
        return number.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".");
    }

    $(document).on('change', '#select-model', function () {
        var selectedValue = $(this).val();
        if (selectedValue != "") {
            connection.invoke("ChangeModel", selectedValue).then(function (res) {
                appendTimeLog(getCurrentDateTime(), "Server", `Server change to model ${selectedValue}`);
                alert("Change model successfully!");
            }).catch(function (err) {
                console.error("Error calling API:", err.toString());
            });
        }
    });

    //====================================================== HANDLE USER EVENT ======================================================
    var pageListResult = 1;
    var totalListResult = 0;
    var totalPage = 0;

    $('.form-search-btn-search').click(function () {
        $('.form-search-btn-search').prop('disabled', true).html('Loading...');
        let fromDate = $('#start-date').val() + ' ' + $('#start-time').val();
        let toDate = $('#end-date').val() + ' ' + $('#end-time').val();
        let model = $('#form-search-model').val();
        pageListResult = 1;

        connection.invoke("SearchData", fromDate, toDate, pageListResult, model)
            .then(function (res) {
                $('#form-search-total-tray-ea').html(formatNumberWithDot(res.totalTray));
                $('#form-search-total-ea').html(`${formatNumberWithDot(res.total)}<span class="">&nbspEA</span>`);
                $('#form-search-total-ok-ea').html(`${formatNumberWithDot(res.totalOK)}<span class= ""> EA</span>`);
                $('#form-search-total-ng-ea').html(`${formatNumberWithDot(res.totalNG)}<span class="">EA</span>`);
                $('#form-search-total-empty-ea').html(`${formatNumberWithDot(res.totalEmpty)}<span class= "" > EA</span >`);
                $('#form-search-percent-ok').html(`${res.percentOK} %`);
                $('#form-search-percent-ng').html(`${res.percentNG} %`);
                $('#form-search-percent-empty').html(`${res.percentEmpty} %`);

                totalListResult = res.total;

                totalPage = Math.ceil(totalListResult / 20);


                if (totalListResult > 40) {
                    $('.form-search-btn-load-more').prop('disabled', false);
                }

                let data = '';
                if (res.results.length > 0) {
                    res.results.forEach(item => {
                        let result = GetResult(item);

                        let err = '';
                        item.errors.forEach(itemErr => {
                            err += ',' + itemErr.description;
                        });

                        let img = '';
                        item.images.forEach(itemImg => {
                            if (itemImg.path.trim() != 'No_save' && itemImg.path.trim() != '') {
                                img += `<a target="_blank" href="${convertPathToUrl(itemImg.path, getUrlBase(itemImg.clientId))}">Image</a>,`;
                            }
                        });

                        data += `
                            <tr style="font-size: 14px; font-weight: 500;">
                                <td>${item.id}</td>
                                <td style="width: 200px">${item.time}</td>
                                <td style="width: 200px">${item.model}</td>
                                <td>${item.tray}</td>
                                <td>${item.index}</td>
                                <td class="${result == 'NG' ? 'text-danger' : 'text-success'}">${result}</td>
                                <td>${result != 'NG' ? '-' : err.replace(/^,+|,+$/g, '')}</td>
                                <td>
                                    ${result != 'NG' ? '-' : img.replace(/,+$/, '') }
                                </td>
                            </tr>
                        `;
                    });

                    $('#action-form-search #list-result tbody').html('').append(data);
                } else {
                    $('#action-form-search #list-result tbody').html('').append(
                        `<tr>
                            <td colspan="12" class="text-center fw-bold text-dark tag-notice">Not Found Data</td>
                        </tr>`
                    );
                }
            })
            .catch(function (err) {
                console.error("Error calling API:", err.toString());
            })
            .finally(function () {
                $('.form-search-btn-search').prop('disabled', false).html('Search');
            });
    });

    $('.form-search-btn-load-more').click(function () {
        $(this).prop('disabled', true).html('Loading...');

        pageListResult++;

        if (pageListResult == totalPage) {
            $(this).prop('disabled', true);
        }

        let fromDate = $('#start-date').val() + ' ' + $('#start-time').val();
        let toDate = $('#end-date').val() + ' ' + $('#end-time').val();
        let model = $('#form-search-model').val();

        connection.invoke("SearchData", fromDate, toDate, pageListResult, model)
            .then(function (res) {

                let data = '';
                if (res.results.length > 0) {
                    res.results.forEach(item => {
                        let result = GetResult(item);

                        let err = '';
                        item.errors.forEach(itemErr => {
                            err += ',' + itemErr.description;
                        });

                        let img = '';
                        item.images.forEach(itemImg => {
                            if (itemImg.path.trim() != 'No_save' && itemImg.path.trim() != '') {
                                img += `<a target="_blank" href="${convertPathToUrl(itemImg.path, getUrlBase(itemImg.clientId))}">Image</a>,`;
                            }
                        });

                        data += `
                            <tr style="font-size: 14px; font-weight: 500;">
                                <td>${item.id}</td>
                                <td style="width: 200px">${item.time}</td>
                                <td style="width: 200px">${item.model}</td>
                                <td>${item.tray}</td>
                                <td>${item.index}</td>
                                <td class="${result == 'NG' ? 'text-danger' : 'text-success'}">${result}</td>
                                <td>${result != 'NG' ? '-' : err.replace(/^,+|,+$/g, '')}</td>
                                <td>
                                    ${result != 'NG' ? '-' : img.replace(/,+$/, '') }
                                </td>
                            </tr>
                        `;
                    });

                    $('#action-form-search #list-result tbody').append(data);
                }
            })
            .catch(function (err) {
                console.error("Error calling API:", err.toString());
            })
            .finally(function () {
                $('.form-search-btn-load-more').prop('disabled', false).html('Load more');
            });
    });

    $("#modalSearch").on('hide.bs.modal', function () {
        $('#form-search-total-tray-ea').html(0);
        $('#form-search-total-ea').html(`0<span class="">EA</span>`);
        $('#form-search-total-ok-ea').html(`0<span class="">EA</span>`);
        $('#form-search-total-ng-ea').html(`0<span class="">EA</span>`);
        $('#form-search-total-empty-ea').html(`0<span class="">EA</span>`);
        $('#form-search-percent-ok').html(`0 %`);
        $('#form-search-percent-ng').html(`0 %`);
        $('#form-search-percent-empty').html(`0 %`);

        $('#action-form-search #list-result tbody').html('').append(
            `<tr>
                <td colspan="12" class="text-center fw-bold text-dark tag-notice">Click search to show data</td>
            </tr>`
        );

        $('#start-date').val(new Date().toISOString().split('T')[0]);
        $('#end-date').val(new Date().toISOString().split('T')[0]);
        $('#start-time').val('00:00');
        $('#end-time').val('23:59');
        $('#form-search-model').val('');

        pageListResult = 1;
        totalListResult = 0;
        totalPage = 0;
        $('.form-search-btn-load-more').prop('disabled', true).html('Load more');
    });

    $('.btn-reload-model').click(function () {

        $('.btn-reload-model').prop('disabled', true).html('Loading...');

        $('#select-model').html(`<option value="" selected disabled>Choose Model</option>`);

        connection.invoke("ReloadModels")
            .then(function (res) {
                appendTimeLog(getCurrentDateTime(), "Server", "Server reload models");
                alert("Reload models successfully!");
            })
            .catch(function (err) {
                alert("Error can not reload models!");
                console.error("Error calling API:", err.toString());
            })
            .finally(function () {
                $('.btn-reload-model').prop('disabled', false).html('Reload');
            });
    });

    $('input[type=radio][name=mode_run]').change(function () {
        let mode = this.value;
        connection.invoke("ChangeModeRun", this.value)
            .then(function (err) {
                alert("You choose mode " + (mode == 1 ? 'Master' : (mode == 2 ? 'Normal' : 'Audit')) + " successfully!");
                $('#modalSetting').modal('hide');
            })
            .catch(function (err) {
                alert("Error can not change mode!");
                console.error("Error calling API:", err.toString());
            });
    });

    const countdownTime = 20;

    function startCountdown($button) {
        let timeRemaining = countdownTime;
        let client = $button.data('client');
        $button.prop('disabled', true);
        $button.text(`Wait ${timeRemaining} seconds`);


        connection.invoke("ResetCamClient", parseInt(client))
            .then((res) => {

            }).catch((err) => {
                alert("Error can not reset cam!");
                console.error("Error reset cam: ", err.toString());
            });

        const countdownInterval = setInterval(function () {
            timeRemaining--;
            $button.text(`Wait ${timeRemaining} seconds`);

            if (timeRemaining <= 1) {
                clearInterval(countdownInterval);
                $button.prop('disabled', false);
                $button.text(`Reset Cam Client ${client}`);
            }
        }, 1000);
    }

    $('.btn-reset-cam').click(function () {
        const $button = $(this);
        startCountdown($button);
    });

    function convertPathToUrl(filePath, urlBase) {
        try {
            const localBasePath = "D:\\SaveResults";
            let relativePath = filePath.replace(localBasePath, "").replace(/\\/g, "/");
            urlBase = urlBase.replace(/\/$/, "");
            return `${urlBase}${relativePath}`.replace(/ /g, '');
        }
        catch (err) {
            console.log('Error cant convert path to URL' + err.message);
            return '';
        }
    }

    function getUrlBase(clientId) {
        switch (clientId) {
            case 1:
                return "http://192.168.1.11:8881";
                break;
            case 2:
                return "http://192.168.1.22:8881";
                break;
            case 3:
                return "http://192.168.1.33:8881";
                break;
            case 4:
                return "http://192.168.1.44:8881";
                break;
            default:
                return "http://192.168.1.11:8881";
                break;
        }
    }

});