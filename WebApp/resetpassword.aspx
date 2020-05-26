<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="resetpassword.aspx.cs" Inherits="WebApp.resetpassword" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="initial-scale=1, maximum-scale=1,user-scalable=no"/>
    <title>Forget Password Request</title>

    <style>
        .button, [type="submit"] {
            width:100%;
            background-color: #4CAF50;
            border: none;
            color: white;
            padding: 10px 15px;
            text-align: center;
            text-decoration: none;
            display: inline-block;
            font-size: 16px;
            margin: 4px 2px;
            cursor: pointer;
        }

        .content {
            left: 50%;
            position: absolute;
            transform: translateX(-50%);
            width: 300px;
        }

        .label {
            text-align:right;
        }

        .textbox {
            width:50%;
        }

        .center {
            width:100%;
            text-align:center;
        }
    </style>
</head>  
<body>
    <div class="content">
        <form id="PasswordResetForm" runat="server">
            <h1 class="center">Reset Password</h1>
            <div>
                <table>
                    <tr>
                        <td class="label">Password</td>
                        <td class="textbox"><asp:TextBox ID="Password" TextMode="Password" runat="server"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td class="label">Confirm Password</td>
                        <td class="textbox"><asp:TextBox ID="PasswordConfirm" TextMode="Password" runat="server"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td colspan="2"><asp:Button ID="Confirm" runat="server" Text="Reset Password" /></td>
                    </tr>
                </table>
            </div>
        </form>

        <div class="center"><asp:Label ID="Message" runat="server" Text=""></asp:Label></div>
    </div>
</body>
</html>
