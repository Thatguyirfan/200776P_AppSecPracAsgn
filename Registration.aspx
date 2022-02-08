<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Registration.aspx.cs" Inherits="_200776P_PracAssignment.Registration" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Registration</title>

    <%--Bootstrap codes--%>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-1BmE4kWBq78iYhFldvKuhfTAU6auU8tT94WrHftjDbrCEXSU1oBoqyl2QvZ6jIW3" crossorigin="anonymous" />
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js" integrity="sha384-ka7Sk0Gln4gmtz2MlQnikT1wXgYsOg+OMhuP+IlRH9sENBO0LRn5q+8nbTov4+1p" crossorigin="anonymous"></script>

    <%-- Google reCaptcha v3 --%>
    <script src="https://www.google.com/recaptcha/api.js?render=6Ldga1UeAAAAAKUy7L-7UfABa0tuMF4jp3y59KUj"></script>


    <style>
        #page-header {
            background-color: rebeccapurple;
            color: white;
            text-align: center;
            padding: 10px;
        }

        #registerBtn {
            font-weight: 600;
        }

        #sub-header {
            border: solid 1px black;
            border-radius: 15px;
            padding: 5px;
        }
    </style>

    <%-- Javascript functions for front-end validation --%>
    <script type="text/javascript">
        function validate() {
            var str = document.getElementById("password").value;
            document.getElementById("password_lbl").classList.remove("text-success");
            document.getElementById("password_lbl").classList.add("text-danger");

            if (str.length < 12) {
                document.getElementById("password_lbl").innerHTML = "Password must be more than 12 characters";
            }

            else if (str.search(/[0-9]/) == -1) {
                document.getElementById("password_lbl").innerHTML = "Password must include at least 1 number";
            }

            else if (str.search(/[A-Z]/) == -1) {
                document.getElementById("password_lbl").innerHTML = "Password must include at least 1 uppercase letter";
            }

            else if (str.search(/[a-z]/) == -1) {
                document.getElementById("password_lbl").innerHTML = "Password must include at least 1 lowercase letter";
            }

            else if (str.search(/[^a-zA-Z0-9\s]/) == -1) {
                document.getElementById("password_lbl").innerHTML = "Password must include at least 1 special character";
            }

            else {
                document.getElementById("password_lbl").innerHTML = "Excellent!";
                document.getElementById("password_lbl").classList.remove("text-danger");
                document.getElementById("password_lbl").classList.add("text-success");
            }
        }

        function validateDOB() {
            var DOB = Date.parse(document.getElementById("DOB").value);

            if (DOB >= Date.now()) {
                document.getElementById("DOB_lbl").innerHTML = "Invalid date selected";
            }
            else {
                document.getElementById("DOB_lbl").innerHTML = "";
            }
        }
    </script>
</head>
<body>
    <div id="page-header">
        <h1>SITConnect</h1>
    </div>

    <div class="container">
        <div class="row justify-content-center text-center mt-3">
            <div id="sub-header" class="col-12">
                <h1>Registration</h1>
            </div>
        </div>
        <form id="registerForm" method="post" runat="server" enctype="multipart/form-data">
            <div class="row">
                <a id="toLoginLink" class="mt-5" href="Login.aspx">&#8592; Back to Login</a>
            </div>
            <div class="row m-3 justify-content-center">
                    <asp:Label ID="registerMsg" class="text-danger" runat="server"></asp:Label>
                </div>
            <div class="form-group row m-5 justify-content-center">
                <label for="fName" class="col-sm-2 col-form-label">First Name:</label>
                <div class="col-sm-6">
                    <input class="form-control" type="text" id="fName" name="fName" placeholder="Enter first name..." title="Enter first name" pattern="[A-Za-z]{1,50}" maxlength="50" required />
                    <asp:Label ID="fName_lbl" class="text-danger" runat="server"></asp:Label>
                </div>    
            </div>
            <div class="form-group row m-5 justify-content-center">
                <label for="lName" class="col-sm-2 col-form-label">Last Name:</label>
                <div class="col-sm-6">
                    <input class="form-control" type="text" id="lName" name="lName" placeholder="Enter last name..." title="Enter last name" pattern="[A-Za-z]{1,50}" maxlength="50" required />
                    <asp:Label ID="lName_lbl" class="text-danger" runat="server"></asp:Label>
                </div>    
            </div>
            <div class="form-group row m-5 justify-content-center">
                <label for="email" class="col-sm-2 col-form-label">Email:</label>
                <div class="col-sm-6">
                    <input class="form-control" type="email" id="email" name="email" placeholder="Enter email..." required />
                    <asp:Label ID="email_lbl" class="text-danger" runat="server"></asp:Label>
                </div>    
            </div>
            <div class="form-group row m-5 justify-content-center">
                <label for="password" class="col-sm-2 col-form-label">Password:</label>
                <div class="col-sm-6">
                    <input class="form-control" type="password" id="password" name="password" placeholder="Enter password..." onkeyup="validate()" required />
                    <asp:Label ID="password_lbl" runat="server"></asp:Label>
                </div>    
            </div>
            <div class="form-group row m-5 justify-content-center">
                <label for="DOB" class="col-sm-2 col-form-label">Date of Birth:</label>
                <div class="col-sm-6">
                    <input class="form-control" type="date" id="DOB" name="DOB" onchange="validateDOB()" required />
                    <asp:Label ID="DOB_lbl" class="text-danger" runat="server"></asp:Label>
                </div>    
            </div>
            <div class="form-group row m-5 justify-content-center">
                <label for="photo" class="col-sm-2 col-form-label">Photo:</label>
                <div class="col-sm-6">
                    <asp:FileUpload ID="photo" class="form-control" runat="server" accept="image/*" required />
                    <asp:Label ID="photo_lbl" class="text-danger" runat="server"></asp:Label>
                </div>    
            </div>
            <div class="form-group row m-5 justify-content-center">
                <label for="CCInfo" class="col-sm-2 col-form-label">Credit Card Number:</label>
                <div class="col-sm-6">
                    <input class="form-control" type="text" id="CCInfo" name="CCInfo" placeholder="Enter credit card number..." title="Enter credit card number" pattern="[0-9]{16}" required />
                    <asp:Label ID="CCInfo_lbl" class="text-danger" runat="server"></asp:Label>
                </div>    
            </div>
            <div class="form-group row m-5 justify-content-center">
                <div class="col-2">
                    <asp:Button ID="registerBtn" class="btn btn-success" Text="Register" runat="server" OnClick="registerBtn_Click" />
                </div>
            </div>
            <input type="hidden" id="g-recaptcha-response" name="g-recaptcha-response"/>
        </form>
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
