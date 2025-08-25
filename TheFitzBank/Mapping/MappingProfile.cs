using AutoMapper;
using TheFitzBankAPI.Application;
using TheFitzBankAPI.Domain;

namespace TheFitzBankAPI.Mapping {
    public class MappingProfile : Profile {
        public MappingProfile() {
            CreateMap<Account, AccountResponse>()
                .ConstructUsing(account => new AccountResponse(
                    account.AccountNumber,
                    account.OwnerName,
                    account.Balance,
                    account.CreatedAt,
                    account.UpdatedAt
                ));
        }
    }
}
