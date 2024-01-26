﻿using Dapper;
using Domain.IRepositories;
using Domain.Models;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using System.Security.Cryptography;

namespace Infrastructure.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly string _connectionString = "Server=.;DataBase=Ramand;Trusted_Connection=True;Encrypt=False;";
        private readonly IUserRepository _userRepository;

        public TokenRepository(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public Token CreateRefreshToken(string refreshToken)
        {
            var userToken = GetRefreshToken(refreshToken);
            userToken.RefreshToken = Guid.NewGuid().ToString();
            userToken.RefreshTokenExp = DateTime.Now.AddDays(30);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var parameters = new DynamicParameters();
                parameters.Add("@Id", userToken.Id, DbType.Int32);
                parameters.Add("@RefreshToken", userToken.RefreshToken, DbType.String);
                parameters.Add("@RefreshTokenExp", userToken.RefreshTokenExp, DbType.DateTime);

                connection.Execute("UpdateRefreshToken", parameters, commandType: CommandType.StoredProcedure);
            }

            return userToken;
        }

        public Token GetRefreshToken(string refreshToken)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var parameters = new DynamicParameters();
                parameters.Add("@RefreshToken", refreshToken, DbType.String);

                return connection.QueryFirstOrDefault<Token>("FindRefreshToken", parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public bool SaveToken(int userId, Token token)
        {
            if (!string.IsNullOrWhiteSpace(token.JwtToken))
            {
                var algorithm = new SHA256CryptoServiceProvider();
                var byteValue = Encoding.UTF8.GetBytes(token.JwtToken);
                var hashbyte = Convert.ToBase64String(algorithm.ComputeHash(byteValue));
                var user = _userRepository.GetUserBy(userId);
                if (user != null)
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();

                        var parameters = new DynamicParameters();
                        parameters.Add("@Id", userId, DbType.Int32);
                        parameters.Add("@Token", hashbyte, DbType.String);
                        parameters.Add("@Expire", token.Expire, DbType.DateTime);
                        parameters.Add("@RefreshToken", token.RefreshToken, DbType.String);
                        parameters.Add("@RefreshTokenExp", token.RefreshTokenExp, DbType.DateTime);

                        int rowsAffected = connection.Execute("InsertUserToken", parameters, commandType: CommandType.StoredProcedure);
                        return rowsAffected > 0;
                    }
                }
            }
            return false;
        }
    }
}