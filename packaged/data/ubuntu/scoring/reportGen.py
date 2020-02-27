mainHtml = """
<!DOCTYPE html>
<html>
<head>
<title>Scoring Report</title>
<script>
setInterval(function(){ location.reload(); }, INTERVAL);
</script>
</head>
<body>
	<center>
		<img src="icon.png" style="width:120px;height:120px;"/>
		<h1>CyberPatriot Practice Image</h1>
        NOTIF_HERE
		<h3>Created by Nikita Kasumov for SHS CyberPatriot</h2>
		<h4>NET_PTS out of MAX_PTS points received</h3>
	</center>
	<h5>CUR_LOSS pentalties assessed, for a loss of LOSS_PTS points:</h5>
	<div style="color:red;">
	PENALTIES_HERE
	</div>
	<h5>CUR_GAIN out of MAX_GAIN scored security issues fixed, for a gain of CUR_PTS points:</h5>
	VULNS_HERE
</body>
</html>
"""

def generateReport(path, texts, pTexts, points, maxPoints, maxVulns, lossPts, interval, notif):
	vulns = "\n"
	penalties = "\n"
	vulnCount = 0
	penCount = 0
	for text in pTexts:
		penalties += "<i>" + text + "</i><br/>\n"
		penCount += 1
	for text in texts:
		vulns += "<i>" + text + "</i><br/>\n"
		vulnCount += 1
	report = mainHtml.replace("CUR_PTS", str(points)).replace("MAX_PTS", str(maxPoints)) \
        .replace("NOTIF_HERE", "<h3 style='color:red;'>" + notif + "</h3>" if notif != "" else "") \
		.replace("CUR_GAIN", str(vulnCount)).replace("MAX_GAIN", str(maxVulns)).replace("VULNS_HERE", vulns) \
		.replace("INTERVAL", str(interval * 1000 // 2)).replace("CUR_LOSS", str(penCount)).replace("LOSS_PTS", str(lossPts)) \
		.replace("NET_PTS", str(points - lossPts)).replace("PENALTIES_HERE", penalties)
	file = open(path, "w")
	file.write(report)
