using Microsoft.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;

namespace ResearchOrm.Services
{
    public class ClassCreator
    {
        public void CreateClass()
        {
            string codeFileName = "Role.cs";
            var codeCompileUnit = CreateUnit();
            var codeDomProvider = new CSharpCodeProvider();

            //var compile = codeDomProvider.CompileAssemblyFromDom(compilerParameters);
            //Activator.CreateInstance(compile.CompiledAssembly.FullName, compile.CompiledAssembly.GetType("Role").Name);

            var tw = new IndentedTextWriter(new StreamWriter(codeFileName, false), "    ");
            codeDomProvider.GenerateCodeFromCompileUnit(codeCompileUnit, tw, new CodeGeneratorOptions());
            tw.Close();
        }

        private CodeCompileUnit CreateUnit()
        {
            CodeCompileUnit compileUnit = new CodeCompileUnit();

            CodeNamespace samples = new CodeNamespace("ResearchOrm.Models");
            compileUnit.Namespaces.Add(samples);

            var newType = new CodeTypeDeclaration("Role");
            var snippet = new CodeSnippetTypeMember();

            snippet.Text = @"
        public int Id { get; set; }";

            newType.Members.Add(snippet);

            //CodeTypeDeclaration newClass = new CodeTypeDeclaration("Role");
            samples.Types.Add(newType);

            //CodeMemberField field1 = new CodeMemberField("System.String", "myField");
            //class1.Members.Add(field1);

            //CodeMemberProperty property = new CodeMemberProperty();
            //property.Name = "Id";
            //property.Type = new CodeTypeReference("System.Int32");
            //property.Attributes = MemberAttributes.Public;
            //property.GetStatements.Add(new CodeMethodReturnStatement());
            //property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(), new CodePropertySetValueReferenceExpression()));
            //newClass.Members.Add(property);

            //CodeMemberMethod method = new CodeMemberMethod();
            //method.Name = "MyMethod";
            //method.ReturnType = new CodeTypeReference("System.String");
            //method.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "myParameter"));
            //method.Statements.Add(new CodeMethodReturnStatement(new CodeArgumentReferenceExpression("myParameter")));
            //newClass.Members.Add(method);

            return compileUnit;
        }
    }
}
