/*
* ISQLWeb:  Pinify Code
* Author:  Erin D. Rakickas
* Last Updated:  6/1/2011
*
* Details:  This code enables the pinify options for IE9 and higher.
*/
/// <reference path="jquery-1.6.js" />
/// <reference path="jquery-1.6-vsdoc.js" />
/// <reference path="jquery.pinify.js" />

var updateInterval = 10000; //update once a minute
var updateIntervalId;  //No idea what this is used for, other than as a return value for setInterval
var alertStatus = ["None", "Info", "Warning", "Complete"];
var alertIcon = ["", "~/Content/img/Annotate_info.ico", "~/Content/img/Annotate_HighPriority.ico", "~/Content/img/Annotate_Default.ico"];
var AlertState = 0;

updateIntervalId = setInterval(UpdateOverlay, updateInterval);

function UpdateOverlay() {
    $.pinify.clearOverlay();

    if (++AlertState >= 4)
        AlertState = 0;

    $.pinify.addOverlay(
            {
                title: alertStatus[AlertState],
                icon: alertIcon[AlertState]
            });
        }

//Create pinify icon
$(document).ready(function()
    {
        // Here we're adding calling pinify and passing it a set of options
        // pinify will then automatically create the correct <meta> tags and
        // insert them into the head of your document
        $("head").pinify(
            {
                applicationName: "SQL Deployment Self Service",
                //favIcon: "http://" + location.host + "/Content/img/favicon.ico",
                favIcon: "/favicon.ico",
                navColor: "#AA0000",
                startUrl: "https://" + location.host,
                tooltip: "SQL Deployment Self Service",           
                //window: "width=1024;height=768"
            }); //$("head").pinify

        // Check if we support pinning, terminate if not
        // Note, need to set the favorite icon first.
        if (!(!!window.external) && ('msIsSiteMode' in window.external))
        {
            return;
        }


        // pinify() can be called with no parameters,
        // if it is, the plugin will use current information available
        // to the browser to set the needed values

        // Now, let's add our list of shortcuts
        var shortsArray = [];
        shortsArray.push(
            {
                name:"Projects",
                url:"https://" + location.host + "/Projects",
                icon: "/favicon.ico"
            });
        shortsArray.push(
            {
                name:"Admin",
                url:"https://" + location.host + "/Admin",
                icon: "/favicon.ico"
            });

            $.pinify.addJumpList(
                {
                    title: "Shortcuts",
                    items: shortsArray
                });

        // Here we're calling the pinify function with the 'pinTeaser' parameter to
        // add a teaser to the site
        $("#container").prepend('<div id="pinify"></div>')
        $("#pinify").pinify('pinTeaser', 
            {    
                type: "topHat",    
                pinText: "Receive notifications of updates to your sql scripts.",    
                linkText:  "Click here to add Self Service SQL to your desktop.",
                style: 
                    {       
                        backgroundImage: "Content/img/banner.png",        
                        closeButtonImage: "Content/img/pig.jpg"    
                    }
                });

//        // Here we're creating an array to store our Pinning Steps
//        var stepsArray = [];
//        // In the example below, we're going to generate a list of Pinning Steps to 
//        // display in our Dynamic Jump List based on an HTML list of Pinning Steps 
//        // here on buildmypinnedsite.com
//        // Since we're using the pinify jQuery plugin, we do not need to use the
//        // try/catch blocks, pinify handles that for us
//        // Here we're grabbing the list of Pinning Steps from an unordered list on this site
//        // and looping through the
//        $("#steps a").each(data, function (key, val) {
//        var $this = $(this);
//        // The name, destination URL and icon of the current Pinning Step
//        // is stored in an object
//        var item = {
//        'name': $this.html(),
//        'url': $this.attr("href"),
//        'icon': "/favicon.ico"
//        };
//        // That object is then added to the array we created to store our Pinning Steps
//        stepsArray.push(item);
//        });
//        // Here we're calling pinify's addJumpList method
//        // It combines both the msSiteModeCreateJumpList and msSiteModeAddJumpListItem
//        // methods and creates a Dynamic Jump List using the tile and array of items below
//        $.pinify.addJumpList({
//        title: "Build My Pinned Site',
//        items: stepsArray
//        });


    }); // $(document).ready