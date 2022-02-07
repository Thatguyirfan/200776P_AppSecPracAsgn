<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="_200776P_PracAssignment.ChangePassword" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Change Password</title>

    <%--Bootstrap codes--%>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-1BmE4kWBq78iYhFldvKuhfTAU6auU8tT94WrHftjDbrCEXSU1oBoqyl2QvZ6jIW3" crossorigin="anonymous" />
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js" integrity="sha384-ka7Sk0Gln4gmtz2MlQnikT1wXgYsOg+OMhuP+IlRH9sENBO0LRn5q+8nbTov4+1p" crossorigin="anonymous"></script>

    <%-- Google reCaptcha v3 --%>
    <script src="https://www.google.com/recaptcha/api.js?render=6Ldga1UeAAAAAKUy7L-7UfABa0tuMF4jp3y59KUj"></script>

    <script type="text/javascript">
        function validate() {
            var str = document.getElementById("newPassword").value;
            document.getElementById("newPassword_lbl").classList.remove("text-success");
            document.getElementById("newPassword_lbl").classList.add("text-danger");

            if (str.length < 12) {
                document.getElementById("newPassword_lbl").innerHTML = "Password must be more than 12 characters";
            }

            else if (str.search(/[0-9]/) == -1) {
                document.getElementById("newPassword_lbl").innerHTML = "Password must include at least 1 number";
            }

            else if (str.search(/[A-Z]/) == -1) {
                document.getElementById("newPassword_lbl").innerHTML = "Password must include at least 1 uppercase letter";
            }

            else if (str.search(/[a-z]/) == -1) {
                document.getElementById("newPassword_lbl").innerHTML = "Password must include at least 1 lowercase letter";
            }

            else if (str.search(/[^a-zA-Z0-9\s]/) == -1) {
                document.getElementById("newPassword_lbl").innerHTML = "Password must include at least 1 special character";
            }

            else {
                document.getElementById("newPassword_lbl").innerHTML = "Excellent!";
                document.getElementById("newPassword_lbl").classList.remove("text-danger");
                document.getElementById("newPassword_lbl").classList.add("text-success");
            }

            //function showDiv() {
            //    var msgLength = document.getElementById("alertMsg_display").innerHTML.length;
            //    console.log(msgLength);
            //    if (msgLength == 0) {
            //        document.getElementById("alertDiv").classList.remove("alert-danger");
            //        document.getElementById("alertDiv").classList.add("hide");
            //    }
            //    else {
            //        document.getElementById("alertDiv").classList.remove("hide");
            //        document.getElementById("alertDiv").classList.add("alert-danger");
            //    }
            //}
        }
    </script>

    <style>
        #page-header {
            background-color: rebeccapurple;
            color: white;
            text-align: center;
            padding: 10px;
        }

        #sub-header {
            border: solid 1px black;
            border-radius: 15px;
            padding: 5px;
        }
    </style>
</head>
<body>
    <div id="page-header">
        <h1>Welcome to SITConnect</h1>
    </div>

    <div class="container">
        <div class="row justify-content-center text-center mt-3">
            <div id="sub-header" class="col-12">
                <h1>Change Password</h1>
            </div>
            <div id="alertDiv" class="alert hide mt-4" role="alert">
              <asp:Label ID="alertMsg_display" runat="server"></asp:Label>
            </div>
            <script type="text/javascript">
                var msgLength = document.getElementById("alertMsg_display").innerHTML.length;
                if (msgLength == 0) {
                    document.getElementById("alertDiv").classList.remove("alert-danger");
                    document.getElementById("alertDiv").classList.add("hide");
                }
                else {
                    document.getElementById("alertDiv").classList.remove("hide");
                    document.getElementById("alertDiv").classList.add("alert-danger");
                }
            </script>
        </div>
        <div class="row justify-content-center">
            <div class="col-8">
                <form id="changePwdForm" method="post" runat="server">
                    <div class="form-group row m-5 justify-content-center">
                    <label for="currPassword" class="col-sm-3 col-form-label">Current password:</label>
                        <div class="col-sm-6">
                            <input class="form-control" type="password" id="currPassword" name="currPassword" placeholder="Enter current password..." />
                        </div>    
                    </div>
                    <div class="form-group row m-5 justify-content-center">
                    <label for="newPassword" class="col-sm-3 col-form-label">New password:</label>
                        <div class="col-sm-6">
                            <input class="form-control" type="password" id="newPassword" name="newPassword" onkeyup="validate()" placeholder="Enter new password..." />
                            <asp:Label ID="newPassword_lbl" runat="server"></asp:Label>
                        </div>    
                    </div>
                    <div class="form-group row m-3 justify-content-center text-center">
                        <asp:Label ID="errorMsg" class="text-danger text-center" runat="server"></asp:Label>
                    </div>
                    <input type="hidden" id="g-recaptcha-response" name="g-recaptcha-response"/>
                    <div class="form-group m-5 text-center">
                        <asp:Button ID="confirmBtn" class="btn btn-success" Text="Confirm change" runat="server" OnClick="confirmBtn_Click" />
                    </div>
                    <div class="form-group m-5 text-center">
                        <a id="toHomeLink" href="Home.aspx">&#8592; Return to home</a> 
                    </div>
                </form>
            </div>
        </div>
    </div>

    <%-- Google reCaptcha v3 --%>
    <script>
        grecaptcha.ready(function () {
            grecaptcha.execute('6Ldga1UeAAAAAKUy7L-7UfABa0tuMF4jp3y59KUj', { action: 'Login' }).then(function (token) {
                document.getElementById("g-recaptcha-response").value = token;
            });
        });
    </script>
</body>
</html>
