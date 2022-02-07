<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="_200776P_PracAssignment.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login</title>

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

        #loginBtn {
            font-weight: 600;
            max-width: 60px;
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
        <h1>SITConnect</h1>
    </div>

    <div class="container">
        <div class="row justify-content-center text-center mt-3">
            <div id="sub-header" class="col-12">
                <h1>Login</h1>
            </div>
        </div>
        <div class="row">
            <form id="loginForm" method="post" runat="server">
                <div class="form-group row m-5 justify-content-center">
                    <label for="email" class="col-sm-2 col-form-label">Email:</label>
                    <div class="col-sm-6">
                        <input class="form-control" type="email" id="email" name="email" placeholder="Enter email..." />
                    </div>    
                </div>
                <div class="form-group row m-5 justify-content-center">
                    <label for="password" class="col-sm-2 col-form-label">Password:</label>
                    <div class="col-sm-6">
                        <input class="form-control" type="password" id="password" name="password" placeholder="Enter password..." />
                    </div>    
                </div>
                <div class="form-group row m-3 justify-content-center text-center">
                    <asp:Label ID="loginMsg" class="text-danger text-center" runat="server"></asp:Label>
                </div>
                <div class="form-group row m-5 justify-content-center">
                    <asp:Button ID="loginBtn" class="btn btn-primary" Text="Login" runat="server" OnClick="loginBtn_Click" />
                </div>
                <input type="hidden" id="g-recaptcha-response" name="g-recaptcha-response"/>
                <div class="form-group row m-5 text-center">
                    <a id="toRegisterLink" href="Registration.aspx">New? Click to register &#8594</a>
                </div>
            </form>
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
