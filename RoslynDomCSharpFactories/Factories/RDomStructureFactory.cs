﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynDom.Common;

namespace RoslynDom.CSharp
{
   internal static class RDomStructureFactoryHelper
   {
      // until move to C# 6 - I want to support name of as soon as possible
      [ExcludeFromCodeCoverage]
      private static string nameof<T>(T value) { return ""; }

      private static WhitespaceKindLookup _whitespaceLookup;

      private static WhitespaceKindLookup whitespaceLookup
      {
         get
         {
            if (_whitespaceLookup == null)
            {
               _whitespaceLookup = new WhitespaceKindLookup();
               _whitespaceLookup.Add(LanguageElement.StructureKeyword, SyntaxKind.StructKeyword);
               _whitespaceLookup.Add(LanguageElement.Identifier, SyntaxKind.IdentifierToken);
               _whitespaceLookup.Add(LanguageElement.StructureStartDelimiter, SyntaxKind.OpenBraceToken);
               _whitespaceLookup.Add(LanguageElement.StructureEndDelimiter, SyntaxKind.CloseBraceToken);
               _whitespaceLookup.AddRange(WhitespaceKindLookup.AccessModifiers);
               _whitespaceLookup.AddRange(WhitespaceKindLookup.Eol);
            }
            return _whitespaceLookup;
         }
      }

      public static RDomStructure CreateFrom(SyntaxNode syntaxNode, IDom parent, SemanticModel model, ICSharpCreateFromWorker createFromWorker, RDomCorporation corporation)
      {
         var syntax = syntaxNode as StructDeclarationSyntax;
         var newItem = new RDomStructure(syntaxNode, parent, model);
         createFromWorker.StandardInitialize(newItem, syntaxNode, parent, model);
         createFromWorker.StoreWhitespace(newItem, syntax, LanguagePart.Current, whitespaceLookup);
         newItem.Name = newItem.TypedSymbol.Name;

         var members = ListUtilities.MakeList(syntax, x => x.Members, x => corporation.Create(x, newItem, model)).OfType< ITypeMemberCommentWhite>();
         newItem.MembersAll.AddOrMoveRange(members);

         return newItem;
      }

      public static IEnumerable<SyntaxNode> BuildSyntax(RDomStructure item, ICSharpBuildSyntaxWorker buildSyntaxWorker, RDomCorporation corporation)
      {
         var itemAsT = item as IStructure;
         Guardian.Assert.IsNotNull(itemAsT, nameof(itemAsT));

         // This is identical to Class, but didn't work out reuse here
         var modifiers = item.BuildModfierSyntax();
         var identifier = SyntaxFactory.Identifier(item.Name);

         var node = SyntaxFactory.StructDeclaration(identifier)
             .WithModifiers(modifiers);
         node = BuildSyntaxHelpers.AttachWhitespace(node, item.Whitespace2Set, whitespaceLookup);

         var baseList = BuildSyntaxHelpers.GetBaseList(item);
         if (baseList != null) { node = node.WithBaseList(baseList); }

         var attributes = buildSyntaxWorker.BuildAttributeSyntax(item.Attributes);
         if (attributes.Any()) { node = node.WithAttributeLists(BuildSyntaxHelpers.WrapInAttributeList(attributes)); }

         var membersSyntax = itemAsT.Members
                     .SelectMany(x => RDom.CSharp.GetSyntaxGroup(x))
                     .ToList();
         node = node.WithMembers(SyntaxFactory.List(membersSyntax));

         node = BuildSyntaxHelpers.BuildTypeParameterSyntax(
                    itemAsT, node, whitespaceLookup,
                    (x, p) => x.WithTypeParameterList(p),
                    (x, c) => x.WithConstraintClauses(c));

         return node.PrepareForBuildSyntaxOutput(item);
      }
   }

   public class RDomStructureTypeMemberFactory
     : RDomTypeMemberFactory<RDomStructure, StructDeclarationSyntax>
   {
      public RDomStructureTypeMemberFactory(RDomCorporation corporation)
          : base(corporation)
      { }

      protected override IEnumerable<IDom> CreateListFromInterim(SyntaxNode syntaxNode, IDom parent, SemanticModel model)
      {
         var ret = RDomStructureFactoryHelper.CreateFrom(syntaxNode, parent, model, CreateFromWorker, Corporation);
         return new IDom[] { ret };
      }

      public override IEnumerable<SyntaxNode> BuildSyntax(IDom item)
      {
         return RDomStructureFactoryHelper.BuildSyntax((RDomStructure)item, BuildSyntaxWorker, Corporation);
      }
   }

   //public class RDomStructureTypeMemberFactory
   //  : RDomTypeMemberFactory<RDomStructure, StructDeclarationSyntax>
   //{
   //   public RDomStructureTypeMemberFactory(RDomCorporation corporation)
   //      : base(corporation)
   //   { }

   //   protected override ITypeMemberCommentWhite CreateItemFrom(SyntaxNode syntaxNode, IDom parent, SemanticModel model)
   //   {
   //      return RDomStructureFactoryHelper.CreateFrom(syntaxNode, parent, model, CreateFromWorker, Corporation);
   //   }
   //   public override IEnumerable<SyntaxNode> BuildSyntax(IDom item)
   //   {
   //      return RDomStructureFactoryHelper.BuildSyntax((RDomStructure)item, BuildSyntaxWorker, Corporation);
   //   }
   //}

   //public class RDomStructureStemMemberFactory
   //       : RDomStemMemberFactory<RDomStructure, StructDeclarationSyntax>
   //{
   //   public RDomStructureStemMemberFactory(RDomCorporation corporation)
   //      : base(corporation)
   //   { }

   //   protected override IStemMemberCommentWhite CreateItemFrom(SyntaxNode syntaxNode, IDom parent, SemanticModel model)
   //   {
   //      return RDomStructureFactoryHelper.CreateFrom(syntaxNode, parent, model, CreateFromWorker, Corporation);
   //   }

   //   public override IEnumerable<SyntaxNode> BuildSyntax(IDom item)
   //   {

   //      return RDomStructureFactoryHelper.BuildSyntax((RDomStructure)item, BuildSyntaxWorker, Corporation);
   //   }
   //}

}
