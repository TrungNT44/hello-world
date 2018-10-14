﻿using System;
using System.Linq;
using System.Web.Security;
using Microsoft.Web.Helpers;
using sThuoc.DAL;
using sThuoc.Models;
using sThuoc.Repositories;
using WebMatrix.WebData;

namespace sThuoc.Filter
{
    public static class WebSecurity 
    {
        public static UserProfile GetUser(string username)
        {
            UnitOfWork uow = new UnitOfWork();
            return uow.UserProfileRepository.Get(u => u.UserName == username).SingleOrDefault();
        }

        public static void UpdateUser(UserProfile user)
        {
            using (UnitOfWork uow = new UnitOfWork())
            {
                uow.UserProfileRepository.Update(user);
                uow.Save();
            }
        }

        public static UserProfile GetCurrentUser()
        {
            return GetUser(CurrentUserName);
        }

        public static void CreateUser(UserProfile user)
        {
            UserProfile dbUser = GetUser(user.UserName);
            if (dbUser != null)
                throw new Exception("Đã có người dùng trong hệ thống.");
            UnitOfWork uow = new UnitOfWork();
            uow.UserProfileRepository.Insert(user);
            uow.Save();
        }

        public static bool FoundUser(string username)
        {
            UnitOfWork uow = new UnitOfWork();
            UserProfile user = uow.UserProfileRepository.Get(u => u.UserName == username).SingleOrDefault();
            return user != null;
        }

        public static string GetEmail(string username)
        {
            string email = null;
            UnitOfWork uow = new UnitOfWork();
            UserProfile user = uow.UserProfileRepository.Get(u => u.UserName == username).SingleOrDefault();
            if (user != null)
                email = user.Email;
            return email;
        }

        public static void Register()
        {
            SecurityContext context = new SecurityContext();
            context.Database.Initialize(false);
            if (!WebMatrix.WebData.WebSecurity.Initialized)
                WebMatrix.WebData.WebSecurity.InitializeDatabaseConnection("SimpleSecurityConnection",
                    "UserProfile", "UserId", "UserName", autoCreateTables: true);
        }

        public static bool ValidateUser(string userName, string password)
        {
            var membership = (SimpleMembershipProvider)Membership.Provider;
            return membership.ValidateUser(userName, password);

        }

        public static bool Login(string userName, string password, bool persistCookie = false)
        {
            return WebMatrix.WebData.WebSecurity.Login(userName, password, persistCookie);
        }

        public static bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            return WebMatrix.WebData.WebSecurity.ChangePassword(userName, oldPassword, newPassword);
        }

        public static bool ConfirmAccount(string accountConfirmationToken)
        {
            return WebMatrix.WebData.WebSecurity.ConfirmAccount(accountConfirmationToken);
        }

        public static void CreateAccount(string userName, string password, bool requireConfirmationToken = false)
        {
            WebMatrix.WebData.WebSecurity.CreateAccount(userName, password, requireConfirmationToken);
        }

        public static string CreateUserAndAccount(string userName, string password, string tenDayDu,string email, string soDienThoai,string soCMT, string maNhaThuoc = null,bool requireConfirmationToken = false)
        {
            return WebMatrix.WebData.WebSecurity.CreateUserAndAccount(userName, password, new {TenDayDu= tenDayDu, Email = email, SoDienThoai = soDienThoai, MaNhaThuoc = maNhaThuoc, HoatDong=1, SoCMT = soCMT }, requireConfirmationToken);
        }

        public static int GetUserId(string userName)
        {
            return WebMatrix.WebData.WebSecurity.GetUserId(userName);
        }

        public static void Logout()
        {
            WebMatrix.WebData.WebSecurity.Logout();
        }

        public static bool IsAuthenticated { get { return WebMatrix.WebData.WebSecurity.IsAuthenticated; } }

        public static bool IsConfirmed(string username)
        {
            return WebMatrix.WebData.WebSecurity.IsConfirmed(username);
        }

        public static string CurrentUserName { get { return WebMatrix.WebData.WebSecurity.CurrentUserName; } }
        public static string CurrentUserFullName
        {
            get
            {
                var fullName = CurrentUserName;
                var user = GetUser(CurrentUserName);
                if (user != null)
                {
                    fullName = user.TenDayDu;
                }

                return fullName;
            }
        }

        public static int GetCurrentUserId { get { return WebMatrix.WebData.WebSecurity.CurrentUserId; } }        

        public static bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            var roleProvider = (SimpleRoleProvider)Roles.Provider;
            var membership = (SimpleMembershipProvider)Membership.Provider;
            if (deleteAllRelatedData)
            {
                string[] roles = roleProvider.GetRolesForUser(username);
                if (roles.Length > 0)
                {
                    string[] users = { username };
                    roleProvider.RemoveUsersFromRoles(users, roles);
                }
                membership.DeleteAccount(username);
            }
            bool wasDeleted = membership.DeleteUser(username, true);
            return wasDeleted;
        }

        public static string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow = 1440)
        {
            return WebMatrix.WebData.WebSecurity.GeneratePasswordResetToken(userName, tokenExpirationInMinutesFromNow);
        }

        public static bool ResetPassword(string passwordResetToken, string newPassword)
        {
            return WebMatrix.WebData.WebSecurity.ResetPassword(passwordResetToken, newPassword);
        }
        public static bool ResetPasswordForce(string userName, string newPassword)
        {
            var token = GeneratePasswordResetToken(userName);
            return WebMatrix.WebData.WebSecurity.ResetPassword(token, newPassword);
        }

        public static string GetConfirmationToken(string userName)
        {
            UnitOfWork uow = new UnitOfWork();
            int userId = uow.UserProfileRepository.Get(u => u.UserName == userName).Select(x => x.UserId).SingleOrDefault();
            string token = uow.MembershipRepository.GetConfirmationToken(userId);
            return token;
        }
    }
}
