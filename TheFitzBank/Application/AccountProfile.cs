using AutoMapper;
using TheFitzBankAPI.Domain;

namespace TheFitzBankAPI.Application {
    public class AccountProfile : Profile {
        public AccountProfile() {
            CreateMap<Account, AccountResponse>();
        }
    }
}
