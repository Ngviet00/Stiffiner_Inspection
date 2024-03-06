"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/homeHub").build();

$(function () {
    connection.start().then(function () {
        console.log("connect success");
    }).catch(function (err) {
        return console.error(err.toString());
    });
});