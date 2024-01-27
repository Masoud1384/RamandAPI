﻿using Application.UserOperations.Commands;

namespace Application.UserOperations.IRepositoryApplication
{
    public interface IUserRepositoryApplication
    {
        IEnumerable<UserVM> GetAll();
        bool Delete(string username);
        UserVM Create(CreateUserCommand user);

        UserVM GetUserBy(int id);
        UserVM GetUserBy(string username);

        bool IsUsernameExist(string username);

    }
}
