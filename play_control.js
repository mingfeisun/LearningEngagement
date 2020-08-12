// var vid_skel = videojs('vid_skel');
var id_vid_real 		= ["vid_real_1", "vid_real_2", "vid_real_3", "vid_real_4"];
var id_vid_sim 			= ["vid_sim_1", "vid_sim_2", "vid_sim_3", "vid_sim_4"];
var id_panel_video 		= ["panel_video_1", "panel_video_2", "panel_video_3", "panel_video_4"];
var id_panel_question 	= ["panel_question_1", "panel_question_2", "panel_question_3", "panel_question_4"];
var id_btn_sim 			= ["btn_sim_1", "btn_sim_2", "btn_sim_3", "btn_sim_4"];
var id_btn_fin 			= ["btn_fin_1", "btn_fin_2", "btn_fin_3", "btn_fin_4"];

var vid_real 		= [];
var vid_sim 		= [];
var panel_video 	= [];
var panel_question 	= [];
var btn_sim 		= [];
var btn_fin 		= [];

var n_clicks        = [];

for (var i = 0; i < id_vid_real.length; i++) {
	vid_real.push(videojs(id_vid_real[i]));
	vid_sim.push(videojs(id_vid_sim[i]));

	panel_video.push(document.getElementById(id_panel_video[i]));
	panel_question.push(document.getElementById(id_panel_question[i]));
	btn_sim.push(document.getElementById(id_btn_sim[i]));
	btn_fin.push(document.getElementById(id_btn_fin[i]));

	n_clicks.push(0);
}

//must input personal info
function checkFormInfo(objData){
	for (var i = 0; i < objData.length; i++) {
		if (objData[i].value == ""){
			alert("Please input value: " + objData[i].name);
			return false;
		}
	}
	return true;
}

//after watching, must input sport type
function checkFormSportType(objData){
	if (objData.length == 0) {
		alert("Please pick the sport type in Step 1");
		return false;
	}
	for (var i = 0; i < objData.length; i++) {
		if (objData[i].value == ""){
			alert("Please pick the sport type in Step 1");
			return false;
		}
	}

	return true;
}


//must input personal info
function checkIterationFormInfo(objData){
	if (objData.length == 0) {
		alert("Please answer questions after watching");
		return false;
	}

	for (var i = 0; i < objData.length; i++) {
		if (objData[i].value == ""){
			alert("Please answer question: " + objData[i].name);
			return false;
		}
	}
	return true;
}


//must answer all questions
function checkQuestionRating(objData){
	if (objData.length == 0) {
		alert("Please rate the robot learning performances in Step 2");
		return false;
	}

	var items = ["attention", "mimic", "engagement", "master", "why", "acceptable", "intelligence", "demonstration", "comments"];
	if (objData.length != 9){
		var num = objData.length+1;
		for (var i = 0; i < objData.length; i++) {
			if (objData[i].name != items[i]){
				num = i;
				break;
			}
		}
		if (num < 4){
			alert("Please answer Q" + (i+1) + " in Step 2");
			return false;
		}
		if (num == 4){
			alert("Please answer why question for Q4 in Step 2");
			return false;
		}
		if (num == 8){
			return true;
		}
		if (num > 4){
			alert("Please answer Q" + i + " in Step 2");
			return false;
		}
	}
	for (var i = 0; i < objData.length; i++) {
		if (objData[i].value == ""){
			if (i < 4){
				alert("Please answer Q" + (i+1) + " in Step 2");
				return false;
			}
			if (i == 4){
				alert("Please answer why question for Q4 in Step 2");
				return false;
			}
			if (i == 8){
				return true;
			}
			if (i > 4){
				alert("Please answer Q" + i + " in Step 2");
				return false;
			}
		}
	}

	return true;
}

/*
function checkQuestionOverall(objData){
	if (objData.length == 0) {
		alert("Please rate the robot learning outcome in Step 3");
		return false;
	}

	var items = ["outcome", "expectation", "why"];
	if (objData.length != 3){
		var num = objData.length + 1;
		for (var i = 0; i < objData.length; i++) {
			if (objData[i].name != items[i]){
				num = i+1;
				break;
			}
		}
		alert("Please answer Q" + (num+4) + " in Step 3");
		return false;
	}

	for (var i = 0; i < objData.length; i++) {
		if (objData[i].value == ""){
			alert("Please answer Q" + (i+5) + " in Step 3");
			return false;
		}
	}


	return true;
}
*/

var data_info = null;
//save the sport type
var data_man_1 = null;
var data_man_2 = null;
var data_man_3 = null;
var data_man_4 = null;
//save the questions
var data_question_1_rating = null;
var data_question_2_rating = null;
var data_question_3_rating = null;
var data_question_4_rating = null;

//save the question in many iterations
var question_collection_1 = null;
var question_collection_2 = null;
var question_collection_3 = null;

/*
var data_question_1_overall = null;
var data_question_2_overall = null;
var data_question_3_overall = null;
var data_question_4_overall = null;
*/


$("#pager-0").click(function(){
	var objData = $("#form_info").serializeArray();
	if(!checkFormInfo(objData)){
		return;
	}
	else{
		data_info = objData;
	}
    $(".nav-tabs a[href='#newdemo1']").tab('show');
	$("html, body").animate({ scrollTop: 0 }, "slow");
	$('#popupPage-1').modal('show');
	setTimeout(function() {
	    $('#popupPage-1').modal('hide');
	}, 2000);
});

$("#pager-1").click(function(){
	var objDataMan = $("#panel_man_1").serializeArray();
	if (!checkFormSportType(objDataMan)) {
		return;
	}
	else{
		data_man_1 = objDataMan;
	}

	var objDataQRating = $("#panel_question_1_1").serializeArray();
	if (!checkQuestionRating(objDataQRating)){
		return;
	}
	else{
		data_question_1_rating = objDataQRating;
	}

	/*
	var objDataQOverall = $("#panel_question_1_2").serializeArray();
	if (!checkQuestionOverall(objDataQOverall)){
		return;
	}
	else{
		data_question_1_overall = objDataQOverall;
	}
	*/

    $(".nav-tabs a[href='#demo2']").tab('show');
	$("html, body").animate({ scrollTop: 0 }, "slow");

	$('#popupPage-2').modal('show');
	setTimeout(function() {
	    $('#popupPage-2').modal('hide');
	}, 2000);
});

$("#pager-2").click(function(){
	var objDataMan = $("#panel_man_2").serializeArray();
	if (!checkFormSportType(objDataMan)) {
		return;
	}
	else{
		data_man_2 = objDataMan;
	}

	var objDataQRating = $("#panel_question_2_1").serializeArray();
	if (!checkQuestionRating(objDataQRating)){
		return;
	}
	else{
		data_question_2_rating = objDataQRating;
	}

	/*
	var objDataQOverall = $("#panel_question_2_2").serializeArray();
	if (!checkQuestionOverall(objDataQOverall)){
		return;
	}
	else{
		data_question_2_overall = objDataQOverall;
	}
	*/

    $(".nav-tabs a[href='#demo3']").tab('show');
	$("html, body").animate({ scrollTop: 0 }, "slow");
	
	$('#popupPage-3').modal('show');
	setTimeout(function() {
	    $('#popupPage-3').modal('hide');
	}, 2000);
});

$("#pager-3").click(function(){
	var objDataMan = $("#panel_man_3").serializeArray();
	if (!checkFormSportType(objDataMan)) {
		return;
	}
	else{
		data_man_3 = objDataMan;
	}

	var objDataQRating = $("#panel_question_3_1").serializeArray();
	if (!checkQuestionRating(objDataQRating)){
		return;
	}
	else{
		data_question_3_rating = objDataQRating;
	}

	/*
	var objDataQOverall = $("#panel_question_3_2").serializeArray();
	if (!checkQuestionOverall(objDataQOverall)){
		return;
	}
	else{
		data_question_3_overall = objDataQOverall;
	}
	*/

    $(".nav-tabs a[href='#demo4']").tab('show');
	$("html, body").animate({ scrollTop: 0 }, "slow");
	
	$('#popupPage-4').modal('show');
	setTimeout(function() {
	    $('#popupPage-4').modal('hide');
	}, 2000);
});

$("#pager-4").click(function(){
	var objDataMan = $("#panel_man_4").serializeArray();
	if (!checkFormSportType(objDataMan)) {
		return;
	}
	else{
		data_man_4 = objDataMan;
	}

	var objDataQRating = $("#panel_question_4_1").serializeArray();
	if (!checkQuestionRating(objDataQRating)){
		return;
	}
	else{
		data_question_4_rating = objDataQRating;
	}

	/*
	var objDataQOverall = $("#panel_question_4_2").serializeArray();
	if (!checkQuestionOverall(objDataQOverall)){
		return;
	}
	else{
		data_question_4_overall = objDataQOverall;
	}
	*/

    // $(".nav-tabs a[href='#demo4']").tab('show');
	// $("html, body").animate({ scrollTop: 0 }, "slow");
	submitData();

	$("#finishModal").modal('show');
});

function submitData(){
	var xhr = new XMLHttpRequest();
	//var url = "http://165.227.108.67/mingfei/submit.php"
	var url = "http://172.17.130.40/userstudy/submit.php"
	xhr.open("POST", url, true);
	xhr.setRequestHeader("Content-Type", "application/json");
	// xhr.setRequestHeader("Access-Control-Allow-Origin", "*");
	xhr.onreadystatechange = function () {
		if (xhr.readyState === 4 && xhr.status === 200) {
			document.getElementById('user_id').innerHTML = xhr.responseText;
	        // console.log(xhr.responseText);
	    }
	};
	data = {"uid": user_id,
			"info": data_info,
			"mode": currMode,
			"sport": currSport,
			"man_1": data_man_1, 
			"man_2": data_man_2, 
			"man_3": data_man_3, 
			"man_4": data_man_4, 
			"q1_rating": data_question_1_rating,
			"q2_rating": data_question_2_rating,
			"q3_rating": data_question_3_rating,
			"q4_rating": data_question_4_rating
			/*
			"q1_overall": data_question_1_overall,
			"q2_overall": data_question_2_overall,
			"q3_overall": data_question_3_overall,
			"q4_overall": data_question_4_overall
			*/
		}
	var dataJson = JSON.stringify(data);
	xhr.send(dataJson);
}

$("#pager-1-prev").click(function(){
    $(".nav-tabs a[href='#home']").tab('show');
	$("html, body").animate({ scrollTop: 0 }, "slow");
});

$("#pager-2-prev").click(function(){
    $(".nav-tabs a[href='#demo1']").tab('show');
	$("html, body").animate({ scrollTop: 0 }, "slow");
});

$("#pager-3-prev").click(function(){
    $(".nav-tabs a[href='#demo2']").tab('show');
	$("html, body").animate({ scrollTop: 0 }, "slow");
});

$("#pager-4-prev").click(function(){
    $(".nav-tabs a[href='#demo3']").tab('show');
	$("html, body").animate({ scrollTop: 0 }, "slow");
});



function openModal1(){
	document.getElementById('div_sim_10').style.display = "block";
}

var question1_1 = null;
var question1_2 = null;
var question1_3 = null;
var question1_4 = null;
var question1_5 = null;
var question1_final = null;


$("#next-1-1").click(function(){
	var objData = $("#learning1_iteration_question_1").serializeArray();
	console.log(objData);
	if(!checkIterationFormInfo(objData)){
		return;
	}
	else{
		question1_1 = objData;
		//data_info = objData;
		//question_collection_1.interation1 = objData;
		//var temp = null;
		//for(var i=0;i<objData.length;i++){
		//	temp.push(objData[i]);
		//}
		//question_collection_1.push(temp);
		console.log(question1_1);
		//缺少存储json数据的代码，后面也是一样
	}

	document.getElementById('learning1_demonscroll-1').style.display = 'none';
	document.getElementById('learning1_demonscroll-2').style.display = 'block';
	document.getElementById('learning1_iteration_question_1').style.display = 'none';
});

$("#next-1-2").click(function(){
	var objData = $("#learning1_iteration_question_2").serializeArray();
	console.log(objData);
	if(!checkIterationFormInfo(objData)){
		return;
	}
	else{
		question1_2 = objData;
		//data_info = objData;
		//question_collection_1.interation1 = objData;
		//var temp = null;
		//for(var i=0;i<objData.length;i++){
		//	temp.push(objData[i]);
		//}
		//question_collection_1.push(temp);
		console.log(objData[0]);
		//缺少存储json数据的代码，后面也是一样
	}

	document.getElementById('learning1_demonscroll-2').style.display = 'none';
	document.getElementById('learning1_demonscroll-3').style.display = 'block';
	document.getElementById('learning1_iteration_question_2').style.display = 'none';
});

$("#next-1-3").click(function(){
	var objData = $("#learning1_iteration_question_3").serializeArray();
	console.log(objData);
	if(!checkIterationFormInfo(objData)){
		return;
	}
	else{
		question1_3 = objData;
		//data_info = objData;
		//question_collection_1.interation1 = objData;
		//var temp = null;
		//for(var i=0;i<objData.length;i++){
		//	temp.push(objData[i]);
		//}
		//question_collection_1.push(temp);
		console.log(objData[0]);
		//缺少存储json数据的代码，后面也是一样
	}

	document.getElementById('learning1_demonscroll-3').style.display = 'none';
	document.getElementById('learning1_demonscroll-4').style.display = 'block';
	document.getElementById('learning1_iteration_question_3').style.display = 'none';
});

$("#next-1-4").click(function(){
	var objData = $("#learning1_iteration_question_4").serializeArray();
	console.log(objData);
	if(!checkIterationFormInfo(objData)){
		return;
	}
	else{
		question1_4 = objData;
		//data_info = objData;
		//question_collection_1.interation1 = objData;
		//var temp = null;
		//for(var i=0;i<objData.length;i++){
		//	temp.push(objData[i]);
		//}
		//question_collection_1.push(temp);
		console.log(objData[0]);
		//缺少存储json数据的代码，后面也是一样
	}

	document.getElementById('learning1_demonscroll-4').style.display = 'none';
	document.getElementById('learning1_demonscroll-5').style.display = 'block';
	document.getElementById('learning1_iteration_question_4').style.display = 'none';
});

$("#next-1-5").click(function(){
	var objData = $("#learning1_iteration_question_5").serializeArray();
	console.log(objData);
	if(!checkIterationFormInfo(objData)){
		return;
	}
	else{
		question1_5 = objData;
		//data_info = objData;
		//question_collection_1.interation1 = objData;
		//var temp = null;
		//for(var i=0;i<objData.length;i++){
		//	temp.push(objData[i]);
		//}
		//question_collection_1.push(temp);
		console.log(objData[0]);
		//缺少存储json数据的代码，后面也是一样
	}

	document.getElementById('learning1_demonscroll-5').style.display = 'none';
	document.getElementById('learning1_iteration_question_5').style.display = 'none';
	document.getElementById('learning1_iteration_question_final').style.display = 'block';
});


$("#next-1-final").click(function(){
	var objData = $("#learning1_iteration_question_final").serializeArray();
	console.log(objData);
	if(!checkIterationFormInfo(objData)){
		return;
	}
	else{
		question1_final = objData;
		//data_info = objData;
		//question_collection_1.interation1 = objData;
		//var temp = null;
		//for(var i=0;i<objData.length;i++){
		//	temp.push(objData[i]);
		//}
		//question_collection_1.push(temp);
		console.log(objData[0]);
		//缺少存储json数据的代码，后面也是一样
	}

	//开始第二个动作

	$(".nav-tabs a[href='#newdemo2']").tab('show');
	$("html, body").animate({ scrollTop: 0 }, "slow");
	$('#popupPage-2').modal('show');
	setTimeout(function() {
	    $('#popupPage-2').modal('hide');
	}, 2000);
});


$("#pager-submit").click(function(){
	var objDataMan = $("#learning1_panel_man").serializeArray();
	if (!checkFormSportType(objDataMan)) {
		return;
	}
	else{
		data_man_1 = objDataMan;
	}


	var xhr = new XMLHttpRequest();
	//var url = "http://165.227.108.67/mingfei/submit.php"
	var url = "http://49.232.60.34/userstudy/test.php"
	xhr.open("POST", url, true);
	xhr.setRequestHeader("Content-Type", "application/json");
	// xhr.setRequestHeader("Access-Control-Allow-Origin", "*");
	xhr.onreadystatechange = function () {
		if (xhr.readyState === 4 && xhr.status === 200) {
			document.getElementById('user_id').innerHTML = xhr.responseText;
			console.log(xhr.responseText);
			alert("Your response has been saved successfully!");
	    }
	};

	data = {"uid": user_id,
			"info": data_info,
			"mode": currMode,
			"sport": currSport,
			"man_1": data_man_1,
			"question1_1": question1_1,
			"question1_2": question1_2,
			"question1_3": question1_3,
			"question1_4": question1_4,
			"question1_5": question1_5,
			"question1_final": question1_final
		}

	var dataJson = JSON.stringify(data);
	xhr.send(dataJson);
});

