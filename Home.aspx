<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="_200776P_PracAssignment.Home" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Home</title>

    <%--Bootstrap codes--%>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-1BmE4kWBq78iYhFldvKuhfTAU6auU8tT94WrHftjDbrCEXSU1oBoqyl2QvZ6jIW3" crossorigin="anonymous" />
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js" integrity="sha384-ka7Sk0Gln4gmtz2MlQnikT1wXgYsOg+OMhuP+IlRH9sENBO0LRn5q+8nbTov4+1p" crossorigin="anonymous"></script>

    <style>
        #page-header {
            background-color: rebeccapurple;
            color: white;
            text-align: center;
            padding: 10px;
        }

        .details {
            border: solid 1px black;
            border-radius: 10px;
            margin-top: 50px;
            padding: 30px;
            background-color: rebeccapurple;
            color: white;
            font-size: 15px;
        }

        .details h4 {
            font-size: 2em;
        }

        #photo {
            max-width: 200px;
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
                <h1>Home</h1>
            </div>
        </div>
    </div>
    
    <div class="container details">
        <div class="row">
            <div class="col-3">
                <div class="d-flex flex-column align-items-center text-center">
                    <asp:Image ID="photo" class="rounded-circle" runat="server"/>
                    <asp:Label ID="fullname" class="font-weight-bold" runat="server"></asp:Label>
                    <asp:Label ID="email_title" class="font-weight-bold" runat="server"></asp:Label>
                </div>
            </div>
            <div class="col-5">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <h4 class="text-right">Account Details</h4>
                </div>
                <div class="row mt-2">
                    <div class="col-12">
                        <asp:Label ID="fName_lbl" runat="server">First Name:</asp:Label>
                        <asp:Label ID="fName_display" runat="server"></asp:Label>
                    </div>
                    <div class="col-12 mt-2">
                        <asp:Label ID="lName_lbl" runat="server">Last Name:</asp:Label>
                        <asp:Label ID="lName_display" runat="server"></asp:Label>
                    </div>
                    <div class="col-12 mt-2">
                        <asp:Label ID="email_lbl" runat="server">Email:</asp:Label>
                        <asp:Label ID="email_display" runat="server"></asp:Label>
                    </div>
                    <div class="col-12 mt-2">
                        <asp:Label ID="DOB_lbl" runat="server">Date of Birth:</asp:Label>
                        <asp:Label ID="DOB_display" runat="server"></asp:Label>
                    </div>
                    <div class="col-12 mt-2">
                        <asp:Label ID="CCInfo_lbl" runat="server">Credit Card Number:</asp:Label>
                        <asp:Label ID="CCInfo_display" runat="server"></asp:Label>
                    </div>
                </div>
            </div>
            <div class="col-4">
                <div class="col-12">
                    <form runat="server" method="post">
                        <asp:Button ID="changePwdBtn" class="btn btn-primary" Text="Change password" runat="server" OnClick="changePwdBtn_Click" />
                        <asp:Button ID="signOutBtn" class="btn btn-danger" runat="server" Text="Sign out" OnClick="signOutBtn_Click" />   
                    </form> 
                </div>
            </div>
        </div>
    </div>
</body>
</html>
