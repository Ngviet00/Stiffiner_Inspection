"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/homeHub").build();

$(function () {
    connection.start().then(function () {
        alert('Connected to dashboardHub');

/*        InvokeProducts();
        InvokeSales();
        InvokeCustomers();*/

    }).catch(function (err) {
        return console.error(err.toString());
    });
});