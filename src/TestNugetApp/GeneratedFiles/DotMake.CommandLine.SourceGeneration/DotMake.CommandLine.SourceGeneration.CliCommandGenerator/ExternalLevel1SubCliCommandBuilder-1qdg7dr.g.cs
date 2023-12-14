﻿// <auto-generated />
// Generated by DotMake.CommandLine.SourceGeneration v1.0.0.0
// Roslyn (Microsoft.CodeAnalysis) v4.800.23.57201
// Time: 2023-12-14T17:14:35.0692282+03:00, Generation: 1

namespace TestApp.Commands.External
{
	public class ExternalLevel1SubCliCommandBuilder : DotMake.CommandLine.DotMakeCommandBuilder
	{
		public ExternalLevel1SubCliCommandBuilder()
		{
			DefinitionType = typeof(TestApp.Commands.External.ExternalLevel1SubCliCommand);
			ParentDefinitionType = typeof(TestApp.Commands.RootWithExternalChildrenCliCommand);
			NameCasingConvention = DotMake.CommandLine.DotMakeCliCasingConvention.KebabCase;
			NamePrefixConvention = DotMake.CommandLine.DotMakeCliPrefixConvention.DoubleHyphen;
			ShortFormPrefixConvention = DotMake.CommandLine.DotMakeCliPrefixConvention.SingleHyphen;
			ShortFormAutoGenerate = true;
		}

		public override System.CommandLine.Command Build()
		{
			// Command for 'ExternalLevel1SubCliCommand' class
			var command = new System.CommandLine.Command("Level1External")
			{
				Description = "An external level 1 sub-command",
			};

			var defaultClass = new TestApp.Commands.External.ExternalLevel1SubCliCommand();

			// Option for 'Option1' property
			var option0 = new System.CommandLine.Option<string>("--option-1")
			{
				Description = "Description for Option1",
			};
			option0.SetDefaultValue(defaultClass.Option1);
			option0.AddAlias("-o");
			command.Add(option0);

			// Argument for 'Argument1' property
			var argument0 = new System.CommandLine.Argument<string>("argument-1")
			{
				Description = "Description for Argument1",
			};
			argument0.SetDefaultValue(defaultClass.Argument1);
			command.Add(argument0);

			// Add nested or external registered children
			foreach (var child in Children)
			{
				command.Add(child.Build());
			}

			System.CommandLine.Handler.SetHandler(command, context =>
			{
				var targetClass = new TestApp.Commands.External.ExternalLevel1SubCliCommand();

				//  Set the parsed or default values for the options
				targetClass.Option1 = context.ParseResult.GetValueForOption(option0);

				//  Set the parsed or default values for the arguments
				targetClass.Argument1 = context.ParseResult.GetValueForArgument(argument0);

				//  Call the command handler
				targetClass.Run();
			});

			return command;
		}

		[System.Runtime.CompilerServices.ModuleInitializerAttribute]
		public static void Initialize()
		{
			var commandBuilder = new TestApp.Commands.External.ExternalLevel1SubCliCommandBuilder();

			// Register this command builder so that it can be found by the definition class
			// and it can be found by the parent definition class if it's a nested/external child.
			commandBuilder.Register();
		}

		public class Level2SubCliCommandBuilder : DotMake.CommandLine.DotMakeCommandBuilder
		{
			public Level2SubCliCommandBuilder()
			{
				DefinitionType = typeof(TestApp.Commands.External.ExternalLevel1SubCliCommand.Level2SubCliCommand);
				ParentDefinitionType = typeof(TestApp.Commands.External.ExternalLevel1SubCliCommand);
				NameCasingConvention = DotMake.CommandLine.DotMakeCliCasingConvention.KebabCase;
				NamePrefixConvention = DotMake.CommandLine.DotMakeCliPrefixConvention.DoubleHyphen;
				ShortFormPrefixConvention = DotMake.CommandLine.DotMakeCliPrefixConvention.SingleHyphen;
				ShortFormAutoGenerate = true;
			}

			public override System.CommandLine.Command Build()
			{
				// Command for 'Level2SubCliCommand' class
				var command = new System.CommandLine.Command("level-2")
				{
					Description = "A nested level 2 sub-command",
				};

				var defaultClass = new TestApp.Commands.External.ExternalLevel1SubCliCommand.Level2SubCliCommand();

				// Option for 'Option1' property
				var option0 = new System.CommandLine.Option<string>("--option-1")
				{
					Description = "Description for Option1",
				};
				option0.SetDefaultValue(defaultClass.Option1);
				option0.AddAlias("-o");
				command.Add(option0);

				// Argument for 'Argument1' property
				var argument0 = new System.CommandLine.Argument<string>("argument-1")
				{
					Description = "Description for Argument1",
				};
				argument0.SetDefaultValue(defaultClass.Argument1);
				command.Add(argument0);

				// Add nested or external registered children
				foreach (var child in Children)
				{
					command.Add(child.Build());
				}

				System.CommandLine.Handler.SetHandler(command, context =>
				{
					var targetClass = new TestApp.Commands.External.ExternalLevel1SubCliCommand.Level2SubCliCommand();

					//  Set the parsed or default values for the options
					targetClass.Option1 = context.ParseResult.GetValueForOption(option0);

					//  Set the parsed or default values for the arguments
					targetClass.Argument1 = context.ParseResult.GetValueForArgument(argument0);

					//  Call the command handler
					targetClass.Run();
				});

				return command;
			}

			[System.Runtime.CompilerServices.ModuleInitializerAttribute]
			public static void Initialize()
			{
				var commandBuilder = new TestApp.Commands.External.ExternalLevel1SubCliCommandBuilder.Level2SubCliCommandBuilder();

				// Register this command builder so that it can be found by the definition class
				// and it can be found by the parent definition class if it's a nested/external child.
				commandBuilder.Register();
			}
		}
	}
}