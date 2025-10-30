using Application.Payments.Commands;
using Application.Payments.Queries;
using Application.WalletBalances.Commands;
using Application.WalletBalances.Queries;
using Application.WalletJournals.Commands;
using Application.WalletJournals.Queries;
using Application.WalletLedgerEntries.Commands;
using Application.WalletLedgerEntries.Queries;
using Application.Wallets.Commands;
using Application.Wallets.Queries;
using Application.WithdrawalRequests.Commands;
using Application.WithdrawalRequests.Queries;
using Domain.Entities;
using Mapster;

namespace Application.Common.Mapper
{
    public static class MappingConfig
    {
        public static void RegisterMappings(TypeAdapterConfig config)
        {
            config.NewConfig<CreatePaymentCommand, Payment>();
            config.NewConfig<UpdatePaymentCommand, Payment>();
            config.NewConfig<Payment, PaymentResponse>();

            config.NewConfig<CreateWalletCommand, Wallet>();
            config.NewConfig<UpdateWalletCommand, Wallet>();
            config.NewConfig<Wallet, WalletResponse>();

            config.NewConfig<CreateWalletBalanceCommand, WalletBalance>();
            config.NewConfig<UpdateWalletBalanceCommand, WalletBalance>();
            config.NewConfig<WalletBalance, WalletBalanceResponse>();

            config.NewConfig<CreateWalletJournalCommand, WalletJournal>();
            config.NewConfig<UpdateWalletJournalCommand, WalletJournal>();
            config.NewConfig<WalletJournal, WalletJournalResponse>();

            config.NewConfig<CreateWalletLedgerEntryCommand, WalletLedgerEntry>();
            config.NewConfig<UpdateWalletLedgerEntryCommand, WalletLedgerEntry>();
            config.NewConfig<WalletLedgerEntry, WalletLedgerEntryResponse>();

            config.NewConfig<CreateWithdrawalRequestCommand, WithdrawalRequest>();
            config.NewConfig<UpdateWithdrawalRequestStatusCommand, WithdrawalRequest>();
            config.NewConfig<WithdrawalRequest, WithdrawalRequestResponse>();
        }
    }
}
